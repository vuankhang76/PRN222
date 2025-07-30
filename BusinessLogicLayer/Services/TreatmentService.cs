using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TreatmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<Treatment>> GetAllTreatmentsAsync()
        {
            return await _unitOfWork.Treatments.GetWithIncludeAsync(
                t => t.Patient!,
                t => t.Doctor!);
        }

        public async Task<Treatment?> GetTreatmentByIdAsync(int id)
        {
            return await _unitOfWork.Treatments.GetByIdAsync(id);
        }

        public async Task<Treatment?> GetTreatmentWithDetailsAsync(int id)
        {
            return await _unitOfWork.Treatments.GetByIdWithIncludeAsync(id,
                t => t.Patient!,
                t => t.Doctor!,
                t => t.TreatmentStages!,
                t => t.Medications!,
                t => t.Appointments!);
        }

        public async Task<Treatment> CreateTreatmentAsync(Treatment treatment)
        {
            if (!await ValidateTreatmentDataAsync(treatment))
            {
                throw new ArgumentException("Dữ liệu điều trị không hợp lệ");
            }

            if (!await CanPatientStartNewTreatmentAsync(treatment.PatientId))
            {
                throw new InvalidOperationException("Bệnh nhân đang có điều trị khác đang diễn ra");
            }

            treatment.StartDate = DateTime.Now;
            treatment.Status = "Đang điều trị";

            var result = await _unitOfWork.Treatments.AddAsync(treatment);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật TreatmentId cho Appointment nếu có
            if (treatment.AppointmentId.HasValue)
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(treatment.AppointmentId.Value);
                if (appointment != null)
                {
                    appointment.TreatmentId = result.Id;
                    await _unitOfWork.Appointments.UpdateAsync(appointment);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return result;
        }

        public async Task<Treatment> UpdateTreatmentAsync(Treatment treatment)
        {
            if (!await ValidateTreatmentDataAsync(treatment))
            {
                throw new ArgumentException("Dữ liệu điều trị không hợp lệ");
            }

            await _unitOfWork.Treatments.UpdateAsync(treatment);
            await _unitOfWork.SaveChangesAsync();
            return treatment;
        }

        public async Task<bool> DeleteTreatmentAsync(int id)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(id);
            if (treatment == null) return false;

            if (treatment.Status == "Đang điều trị")
            {
                throw new InvalidOperationException("Không thể xóa điều trị đang tiến hành");
            }

            await _unitOfWork.Treatments.DeleteAsync(treatment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Business logic đặc biệt
        public async Task<IEnumerable<Treatment>> GetTreatmentsByPatientAsync(int patientId)
        {
            return await _unitOfWork.Treatments.FindWithIncludeAsync(
                t => t.PatientId == patientId,
                t => t.Patient!,
                t => t.Doctor!);
        }

        public async Task<IEnumerable<Treatment>> GetTreatmentsByDoctorAsync(int doctorId)
        {
            return await _unitOfWork.Treatments.FindWithIncludeAsync(
                t => t.DoctorId == doctorId,
                t => t.Patient!,
                t => t.Doctor!);
        }

        public async Task<IEnumerable<Treatment>> GetActiveTreatmentsAsync()
        {
            return await _unitOfWork.Treatments.FindWithIncludeAsync(
                t => t.Status == "Đang điều trị",
                t => t.Patient!,
                t => t.Doctor!);
        }

        public async Task<IEnumerable<Treatment>> GetCompletedTreatmentsAsync()
        {
            return await _unitOfWork.Treatments.FindAsync(t => t.Status == "Hoàn thành");
        }

        public async Task<IEnumerable<Treatment>> GetTreatmentsByStatusAsync(string status)
        {
            return await _unitOfWork.Treatments.FindAsync(t => t.Status == status);
        }

        public async Task<IEnumerable<Treatment>> GetTreatmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.Treatments.FindAsync(t => 
                t.StartDate >= startDate && t.StartDate <= endDate);
        }

        // Quản lý tiến trình điều trị
        public async Task<bool> StartTreatmentAsync(int treatmentId)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentId);
            if (treatment == null) return false;

            treatment.Status = "Đang điều trị";
            treatment.StartDate = DateTime.Now;
            
            await _unitOfWork.Treatments.UpdateAsync(treatment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteTreatmentAsync(int treatmentId, string results)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentId);
            if (treatment == null) return false;

            treatment.Status = "Hoàn thành";
            treatment.EndDate = DateTime.Now;
            treatment.Outcome = results;
            
            await _unitOfWork.Treatments.UpdateAsync(treatment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PauseTreatmentAsync(int treatmentId, string reason)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentId);
            if (treatment == null) return false;

            treatment.Status = "Tạm dừng";
            treatment.Notes = treatment.Notes + $"\nTạm dừng: {reason}";
            
            await _unitOfWork.Treatments.UpdateAsync(treatment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResumeTreatmentAsync(int treatmentId)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentId);
            if (treatment == null) return false;

            treatment.Status = "Đang điều trị";
            treatment.Notes = treatment.Notes + $"\nTiếp tục điều trị: {DateTime.Now}";
            
            await _unitOfWork.Treatments.UpdateAsync(treatment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTreatmentProgressAsync(int treatmentId, string progress)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentId);
            if (treatment == null) return false;

            // Progress property không tồn tại trong Treatment model - dùng Notes để lưu tiến độ
            treatment.Notes = treatment.Notes + $"\nCập nhật tiến độ ({DateTime.Now}): {progress}";
            
            await _unitOfWork.Treatments.UpdateAsync(treatment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Thống kê và báo cáo
        public async Task<int> GetTotalTreatmentsCountAsync()
        {
            return await _unitOfWork.Treatments.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetTreatmentsByStatusStatisticsAsync()
        {
            var allTreatments = await _unitOfWork.Treatments.GetAllAsync();
            return allTreatments.GroupBy(t => t.Status ?? "Không xác định")
                               .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetTreatmentsByTypeStatisticsAsync()
        {
            var allTreatments = await _unitOfWork.Treatments.GetAllAsync();
            return allTreatments.GroupBy(t => t.TreatmentType ?? "Không xác định")
                               .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetAverageTreatmentDurationAsync()
        {
            var completedTreatments = await _unitOfWork.Treatments.FindAsync(t => 
                t.Status == "Hoàn thành" && t.EndDate.HasValue);
            
            if (!completedTreatments.Any()) return 0;

            var durations = completedTreatments.Select(t => 
                (t.EndDate!.Value - t.StartDate).TotalDays);
            
            return (decimal)durations.Average();
        }

        public async Task<decimal> GetTreatmentSuccessRateAsync()
        {
            var completedTreatments = await _unitOfWork.Treatments.FindAsync(t => t.Status == "Hoàn thành");
            var successfulTreatments = completedTreatments.Count(t => 
                !string.IsNullOrEmpty(t.Outcome) && t.Outcome.Contains("thành công"));
            
            if (!completedTreatments.Any()) return 0;
            
            return (decimal)successfulTreatments / completedTreatments.Count() * 100;
        }

        // Validation
        public async Task<bool> CanPatientStartNewTreatmentAsync(int patientId)
        {
            var activeTreatments = await _unitOfWork.Treatments.FindAsync(t => 
                t.PatientId == patientId && t.Status == "Đang điều trị");
            
            return !activeTreatments.Any();
        }

        public async Task<bool> ValidateTreatmentDataAsync(Treatment treatment)
        {
            // Validate patient exists
            var patient = await _unitOfWork.Patients.GetByIdAsync(treatment.PatientId);
            if (patient == null) return false;

            // Validate doctor exists
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(treatment.DoctorId);
            if (doctor == null) return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(treatment.TreatmentType))
                return false;

            return true;
        }

        public async Task<bool> IsTreatmentActiveAsync(int treatmentId)
        {
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(treatmentId);
            return treatment != null && treatment.Status == "Đang điều trị";
        }
    }
} 