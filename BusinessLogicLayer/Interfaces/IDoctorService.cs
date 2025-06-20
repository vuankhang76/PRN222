using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IDoctorService
    {
        // CRUD cơ bản
        Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
        Task<Doctor?> GetDoctorByIdAsync(int id);
        Task<Doctor> CreateDoctorAsync(Doctor doctor);
        Task<Doctor> UpdateDoctorAsync(Doctor doctor);
        Task<bool> DeleteDoctorAsync(int id);

        // Business logic đặc biệt
        Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(string specialization);
        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();
        Task<IEnumerable<Doctor>> SearchDoctorsByNameAsync(string name);
        
        // Thống kê
        Task<int> GetTotalDoctorsCountAsync();
        Task<Dictionary<string, int>> GetDoctorsBySpecializationStatisticsAsync();
    }
} 