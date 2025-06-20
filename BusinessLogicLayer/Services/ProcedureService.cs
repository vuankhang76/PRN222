using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class ProcedureService : IProcedureService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProcedureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<Procedure>> GetAllProceduresAsync()
        {
            return await _unitOfWork.Procedures.GetAllAsync();
        }

        public async Task<Procedure?> GetProcedureByIdAsync(int id)
        {
            return await _unitOfWork.Procedures.GetByIdAsync(id);
        }

        public async Task<Procedure?> GetProcedureWithDetailsAsync(int id)
        {
            return await _unitOfWork.Procedures.GetByIdWithIncludeAsync(id,
                p => p.TreatmentStage!);
        }

        public async Task<Procedure> CreateProcedureAsync(Procedure procedure)
        {
            if (!await ValidateProcedureDataAsync(procedure))
            {
                throw new ArgumentException("Dữ liệu thủ thuật không hợp lệ");
            }

            if (!await CanScheduleProcedureAsync(procedure.TreatmentStageId, procedure.ScheduledDate))
            {
                throw new InvalidOperationException("Không thể lên lịch thủ thuật vào thời gian này");
            }

            procedure.Status = "Scheduled";
            // CreatedAt property không tồn tại trong Procedure model

            var result = await _unitOfWork.Procedures.AddAsync(procedure);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<Procedure> UpdateProcedureAsync(Procedure procedure)
        {
            if (!await ValidateProcedureDataAsync(procedure))
            {
                throw new ArgumentException("Dữ liệu thủ thuật không hợp lệ");
            }

            await _unitOfWork.Procedures.UpdateAsync(procedure);
            await _unitOfWork.SaveChangesAsync();
            return procedure;
        }

        public async Task<bool> DeleteProcedureAsync(int id)
        {
            var procedure = await _unitOfWork.Procedures.GetByIdAsync(id);
            if (procedure == null) return false;

            if (procedure.Status == "In Progress")
            {
                throw new InvalidOperationException("Không thể xóa thủ thuật đang thực hiện");
            }

            await _unitOfWork.Procedures.DeleteAsync(procedure);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Business logic đặc biệt
        public async Task<IEnumerable<Procedure>> GetProceduresByTreatmentStageAsync(int treatmentStageId)
        {
            return await _unitOfWork.Procedures.FindAsync(p => p.TreatmentStageId == treatmentStageId);
        }

        public async Task<IEnumerable<Procedure>> GetProceduresByTypeAsync(string procedureType)
        {
            // Sử dụng ProcedureName thay vì ProcedureType (không tồn tại)
            return await _unitOfWork.Procedures.FindAsync(p => p.ProcedureName.Contains(procedureType));
        }

        public async Task<IEnumerable<Procedure>> GetProceduresByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.Procedures.FindAsync(p => 
                p.ScheduledDate >= startDate && p.ScheduledDate <= endDate);
        }

        public async Task<IEnumerable<Procedure>> GetCompletedProceduresAsync()
        {
            return await _unitOfWork.Procedures.FindAsync(p => p.Status == "Completed");
        }

        public async Task<IEnumerable<Procedure>> GetPendingProceduresAsync()
        {
            return await _unitOfWork.Procedures.FindAsync(p => p.Status == "Scheduled");
        }

        public async Task<IEnumerable<Procedure>> GetProceduresByStatusAsync(string status)
        {
            return await _unitOfWork.Procedures.FindAsync(p => p.Status == status);
        }

        // Quản lý thủ thuật
        public async Task<bool> StartProcedureAsync(int procedureId)
        {
            var procedure = await _unitOfWork.Procedures.GetByIdAsync(procedureId);
            if (procedure == null) return false;

            if (procedure.Status != "Scheduled")
            {
                throw new InvalidOperationException("Chỉ có thể bắt đầu thủ thuật đã được lên lịch");
            }

            procedure.Status = "In Progress";
            procedure.ActualDate = DateTime.Now;

            await _unitOfWork.Procedures.UpdateAsync(procedure);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteProcedureAsync(int procedureId, string results)
        {
            var procedure = await _unitOfWork.Procedures.GetByIdAsync(procedureId);
            if (procedure == null) return false;

            procedure.Status = "Completed";
            procedure.ActualDate = DateTime.Now;
            procedure.Results = results;

            await _unitOfWork.Procedures.UpdateAsync(procedure);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelProcedureAsync(int procedureId, string reason)
        {
            var procedure = await _unitOfWork.Procedures.GetByIdAsync(procedureId);
            if (procedure == null) return false;

            procedure.Status = "Cancelled";
            procedure.Notes = procedure.Notes + $"\nLý do hủy: {reason}";

            await _unitOfWork.Procedures.UpdateAsync(procedure);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProcedureStatusAsync(int procedureId, string status)
        {
            var procedure = await _unitOfWork.Procedures.GetByIdAsync(procedureId);
            if (procedure == null) return false;

            procedure.Status = status;
            await _unitOfWork.Procedures.UpdateAsync(procedure);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Thống kê và báo cáo
        public async Task<int> GetTotalProceduresCountAsync()
        {
            return await _unitOfWork.Procedures.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetProceduresByTypeStatisticsAsync()
        {
            var allProcedures = await _unitOfWork.Procedures.GetAllAsync();
            return allProcedures.GroupBy(p => p.ProcedureName ?? "Không xác định")
                               .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetProceduresByStatusStatisticsAsync()
        {
            var allProcedures = await _unitOfWork.Procedures.GetAllAsync();
            return allProcedures.GroupBy(p => p.Status ?? "Không xác định")
                               .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetProcedureSuccessRateAsync()
        {
            var totalProcedures = await _unitOfWork.Procedures.CountAsync();
            var successfulProcedures = await _unitOfWork.Procedures.CountAsync(p => 
                p.Status == "Completed" && p.Results != null && p.Results.Contains("thành công"));

            if (totalProcedures == 0) return 0;
            return (decimal)successfulProcedures / totalProcedures * 100;
        }

        public async Task<decimal> GetAverageProcedureDurationAsync()
        {
            var completedProcedures = await _unitOfWork.Procedures.FindAsync(p => 
                p.Status == "Completed" && p.ActualDate.HasValue);

            if (!completedProcedures.Any()) return 0;

            var durations = completedProcedures.Select(p => 
                (p.ActualDate!.Value - p.ScheduledDate).TotalHours);

            return (decimal)durations.Average();
        }

        // Validation
        public async Task<bool> ValidateProcedureDataAsync(Procedure procedure)
        {
            // Validate treatment stage exists
            var treatmentStage = await _unitOfWork.TreatmentStages.GetByIdAsync(procedure.TreatmentStageId);
            if (treatmentStage == null) return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(procedure.ProcedureName))
                return false;

            // Validate scheduled date is not in past
            if (procedure.ScheduledDate < DateTime.Now.Date)
                return false;

            return true;
        }

        public async Task<bool> CanScheduleProcedureAsync(int treatmentStageId, DateTime scheduledDate)
        {
            // Kiểm tra xem có thủ thuật nào khác đã được lên lịch cùng ngày không
            var existingProcedures = await _unitOfWork.Procedures.FindAsync(p => 
                p.TreatmentStageId == treatmentStageId && 
                p.ScheduledDate.Date == scheduledDate.Date &&
                p.Status != "Cancelled");

            // Chỉ cho phép tối đa 2 thủ thuật trong 1 ngày
            return existingProcedures.Count() < 2;
        }

        public async Task<bool> IsProcedureTypeValidForTreatmentAsync(string procedureName, int treatmentStageId)
        {
            // Danh sách thủ thuật hợp lệ cho điều trị hiếm muộn
            var validProcedures = new List<string>
            {
                "IVF", "IUI", "ICSI", "Egg Retrieval", "Embryo Transfer",
                "Hysteroscopy", "Laparoscopy", "Semen Analysis", 
                "Ovulation Induction", "Follicle Monitoring"
            };

            return await Task.FromResult(
                validProcedures.Any(valid => 
                    procedureName.Contains(valid, StringComparison.OrdinalIgnoreCase)));
        }
    }
} 