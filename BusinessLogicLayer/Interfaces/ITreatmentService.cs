using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface ITreatmentService
    {
        // CRUD cơ bản
        Task<IEnumerable<Treatment>> GetAllTreatmentsAsync();
        Task<Treatment?> GetTreatmentByIdAsync(int id);
        Task<Treatment?> GetTreatmentWithDetailsAsync(int id);
        Task<Treatment> CreateTreatmentAsync(Treatment treatment);
        Task<Treatment> UpdateTreatmentAsync(Treatment treatment);
        Task<bool> DeleteTreatmentAsync(int id);

        // Business logic đặc biệt cho điều trị hiếm muộn
        Task<IEnumerable<Treatment>> GetTreatmentsByPatientAsync(int patientId);
        Task<IEnumerable<Treatment>> GetTreatmentsByDoctorAsync(int doctorId);
        Task<IEnumerable<Treatment>> GetActiveTreatmentsAsync();
        Task<IEnumerable<Treatment>> GetCompletedTreatmentsAsync();
        Task<IEnumerable<Treatment>> GetTreatmentsByStatusAsync(string status);
        Task<IEnumerable<Treatment>> GetTreatmentsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Quản lý tiến trình điều trị
        Task<bool> StartTreatmentAsync(int treatmentId);
        Task<bool> CompleteTreatmentAsync(int treatmentId, string results);
        Task<bool> PauseTreatmentAsync(int treatmentId, string reason);
        Task<bool> ResumeTreatmentAsync(int treatmentId);
        Task<bool> UpdateTreatmentProgressAsync(int treatmentId, string progress);

        // Thống kê và báo cáo
        Task<int> GetTotalTreatmentsCountAsync();
        Task<Dictionary<string, int>> GetTreatmentsByStatusStatisticsAsync();
        Task<Dictionary<string, int>> GetTreatmentsByTypeStatisticsAsync();
        Task<decimal> GetAverageTreatmentDurationAsync();
        Task<decimal> GetTreatmentSuccessRateAsync();

        // Validation
        Task<bool> CanPatientStartNewTreatmentAsync(int patientId);
        Task<bool> ValidateTreatmentDataAsync(Treatment treatment);
        Task<bool> IsTreatmentActiveAsync(int treatmentId);
    }
} 