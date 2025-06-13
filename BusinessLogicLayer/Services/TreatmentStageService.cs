using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class TreatmentStageService : ITreatmentStageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TreatmentStageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<TreatmentStage>> GetAllTreatmentStagesAsync()
        {
            return await _unitOfWork.TreatmentStages.GetAllAsync();
        }

        public async Task<TreatmentStage?> GetTreatmentStageByIdAsync(int id)
        {
            return await _unitOfWork.TreatmentStages.GetByIdAsync(id);
        }

        public async Task<TreatmentStage?> GetTreatmentStageWithDetailsAsync(int id)
        {
            return await _unitOfWork.TreatmentStages.GetByIdWithIncludeAsync(id,
                ts => ts.Treatment!,
                ts => ts.Procedures!);
        }

        public async Task<TreatmentStage> CreateTreatmentStageAsync(TreatmentStage treatmentStage)
        {
            if (!await ValidateTreatmentStageDataAsync(treatmentStage))
            {
                throw new ArgumentException("Dữ liệu giai đoạn điều trị không hợp lệ");
            }

            if (!await CanCreateNewStageForTreatmentAsync(treatmentStage.TreatmentId))
            {
                throw new InvalidOperationException("Không thể tạo giai đoạn mới cho điều trị này");
            }

            treatmentStage.Status = "Planned";
            // CreatedAt property không tồn tại trong TreatmentStage model

            var result = await _unitOfWork.TreatmentStages.AddAsync(treatmentStage);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<TreatmentStage> UpdateTreatmentStageAsync(TreatmentStage treatmentStage)
        {
            if (!await ValidateTreatmentStageDataAsync(treatmentStage))
            {
                throw new ArgumentException("Dữ liệu giai đoạn điều trị không hợp lệ");
            }

            await _unitOfWork.TreatmentStages.UpdateAsync(treatmentStage);
            await _unitOfWork.SaveChangesAsync();
            return treatmentStage;
        }

        public async Task<bool> DeleteTreatmentStageAsync(int id)
        {
            var treatmentStage = await _unitOfWork.TreatmentStages.GetByIdAsync(id);
            if (treatmentStage == null) return false;

            if (treatmentStage.Status == "In Progress")
            {
                throw new InvalidOperationException("Không thể xóa giai đoạn đang thực hiện");
            }

            await _unitOfWork.TreatmentStages.DeleteAsync(treatmentStage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Business logic đặc biệt
        public async Task<IEnumerable<TreatmentStage>> GetTreatmentStagesByTreatmentAsync(int treatmentId)
        {
            return await _unitOfWork.TreatmentStages.FindAsync(ts => ts.TreatmentId == treatmentId);
        }

        public async Task<IEnumerable<TreatmentStage>> GetTreatmentStagesByStatusAsync(string status)
        {
            return await _unitOfWork.TreatmentStages.FindAsync(ts => ts.Status == status);
        }

        public async Task<IEnumerable<TreatmentStage>> GetActiveTreatmentStagesAsync()
        {
            return await _unitOfWork.TreatmentStages.FindAsync(ts => ts.Status == "In Progress");
        }

        public async Task<IEnumerable<TreatmentStage>> GetCompletedTreatmentStagesAsync()
        {
            return await _unitOfWork.TreatmentStages.FindAsync(ts => ts.Status == "Completed");
        }

        public async Task<TreatmentStage?> GetCurrentTreatmentStageByTreatmentAsync(int treatmentId)
        {
            var stages = await GetTreatmentStagesByTreatmentAsync(treatmentId);
            return stages.FirstOrDefault(ts => ts.Status == "In Progress");
        }

        public async Task<TreatmentStage?> GetNextTreatmentStageAsync(int currentStageId)
        {
            var currentStage = await _unitOfWork.TreatmentStages.GetByIdAsync(currentStageId);
            if (currentStage == null) return null;

            var treatmentStages = await GetTreatmentStagesByTreatmentAsync(currentStage.TreatmentId);
            return treatmentStages
                .Where(ts => ts.StageOrder > currentStage.StageOrder)
                .OrderBy(ts => ts.StageOrder)
                .FirstOrDefault();
        }

        // Quản lý giai đoạn điều trị
        public async Task<bool> StartTreatmentStageAsync(int stageId)
        {
            var stage = await _unitOfWork.TreatmentStages.GetByIdAsync(stageId);
            if (stage == null) return false;

            if (stage.Status != "Planned")
            {
                throw new InvalidOperationException("Chỉ có thể bắt đầu giai đoạn đã được lên kế hoạch");
            }

            stage.Status = "In Progress";
            stage.StartDate = DateTime.Now;

            await _unitOfWork.TreatmentStages.UpdateAsync(stage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteTreatmentStageAsync(int stageId, string results)
        {
            var stage = await _unitOfWork.TreatmentStages.GetByIdAsync(stageId);
            if (stage == null) return false;

            stage.Status = "Completed";
            stage.EndDate = DateTime.Now;
            stage.Results = results;

            await _unitOfWork.TreatmentStages.UpdateAsync(stage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MoveToNextStageAsync(int currentStageId)
        {
            var currentStage = await _unitOfWork.TreatmentStages.GetByIdAsync(currentStageId);
            if (currentStage == null) return false;

            // Hoàn thành giai đoạn hiện tại
            await CompleteTreatmentStageAsync(currentStageId, "Tự động chuyển giai đoạn");

            // Bắt đầu giai đoạn tiếp theo
            var nextStage = await GetNextTreatmentStageAsync(currentStageId);
            if (nextStage != null)
            {
                await StartTreatmentStageAsync(nextStage.Id);
            }

            return true;
        }

        public async Task<bool> UpdateStageProgressAsync(int stageId, string progress)
        {
            var stage = await _unitOfWork.TreatmentStages.GetByIdAsync(stageId);
            if (stage == null) return false;

            stage.Notes = stage.Notes + $"\nCập nhật tiến độ ({DateTime.Now}): {progress}";

            await _unitOfWork.TreatmentStages.UpdateAsync(stage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PauseTreatmentStageAsync(int stageId, string reason)
        {
            var stage = await _unitOfWork.TreatmentStages.GetByIdAsync(stageId);
            if (stage == null) return false;

            stage.Status = "Paused";
            stage.Notes = stage.Notes + $"\nTạm dừng ({DateTime.Now}): {reason}";

            await _unitOfWork.TreatmentStages.UpdateAsync(stage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResumeTreatmentStageAsync(int stageId)
        {
            var stage = await _unitOfWork.TreatmentStages.GetByIdAsync(stageId);
            if (stage == null) return false;

            if (stage.Status != "Paused")
            {
                throw new InvalidOperationException("Chỉ có thể tiếp tục giai đoạn đã tạm dừng");
            }

            stage.Status = "In Progress";
            stage.Notes = stage.Notes + $"\nTiếp tục ({DateTime.Now})";

            await _unitOfWork.TreatmentStages.UpdateAsync(stage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Thống kê và báo cáo
        public async Task<int> GetTotalTreatmentStagesCountAsync()
        {
            return await _unitOfWork.TreatmentStages.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetTreatmentStagesByStatusStatisticsAsync()
        {
            var allStages = await _unitOfWork.TreatmentStages.GetAllAsync();
            return allStages.GroupBy(ts => ts.Status ?? "Không xác định")
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetAverageStageDurationAsync()
        {
            var completedStages = await _unitOfWork.TreatmentStages.FindAsync(ts => 
                ts.Status == "Completed" && ts.EndDate.HasValue);

            if (!completedStages.Any()) return 0;

            var durations = completedStages.Select(ts => 
                (ts.EndDate!.Value - ts.StartDate).TotalDays);

            return (decimal)durations.Average();
        }

        public async Task<decimal> GetStageSuccessRateAsync()
        {
            var totalStages = await _unitOfWork.TreatmentStages.CountAsync();
            var successfulStages = await _unitOfWork.TreatmentStages.CountAsync(ts => 
                ts.Status == "Completed" && ts.Results != null && ts.Results.Contains("thành công"));

            if (totalStages == 0) return 0;
            return (decimal)successfulStages / totalStages * 100;
        }

        // Validation
        public async Task<bool> ValidateTreatmentStageDataAsync(TreatmentStage treatmentStage)
        {
            // Validate treatment exists
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentStage.TreatmentId);
            if (treatment == null) return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(treatmentStage.StageName))
                return false;

            // Validate stage order
            if (!await IsStageOrderValidAsync(treatmentStage.TreatmentId, treatmentStage.StageOrder))
                return false;

            return true;
        }

        public async Task<bool> CanCreateNewStageForTreatmentAsync(int treatmentId)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentId);
            if (treatment == null || treatment.Status != "Đang điều trị") return false;

            // Kiểm tra xem có giai đoạn nào đang thực hiện không
            var activeStages = await _unitOfWork.TreatmentStages.FindAsync(ts => 
                ts.TreatmentId == treatmentId && ts.Status == "In Progress");

            // Chỉ cho phép tối đa 1 giai đoạn đang thực hiện
            return !activeStages.Any();
        }

        public async Task<bool> IsStageOrderValidAsync(int treatmentId, int stageOrder)
        {
            var existingStages = await GetTreatmentStagesByTreatmentAsync(treatmentId);
            
            // Kiểm tra xem thứ tự giai đoạn có bị trùng không
            return !existingStages.Any(ts => ts.StageOrder == stageOrder);
        }
    }
} 