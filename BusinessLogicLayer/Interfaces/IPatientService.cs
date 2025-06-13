using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IPatientService
    {
        // CRUD cơ bản
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient?> GetPatientWithDetailsAsync(int id);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<Patient> UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int id);

        // Business logic đặc biệt cho quản lý bệnh nhân hiếm muộn
        Task<IEnumerable<Patient>> GetPatientsByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<Patient>> GetPatientsByGenderAsync(string gender);
        Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string name);
        Task<IEnumerable<Patient>> GetPatientsWithTreatmentHistoryAsync();
        Task<IEnumerable<Patient>> GetPatientsWithoutPartnerAsync();
        
        // Báo cáo và thống kê
        Task<int> GetTotalPatientsCountAsync();
        Task<Dictionary<string, int>> GetPatientsByGenderStatisticsAsync();
        Task<Dictionary<int, int>> GetPatientsByAgeGroupStatisticsAsync();
        
        // Validation
        Task<bool> IsEmailUniqueAsync(string email, int? excludePatientId = null);
        Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber, int? excludePatientId = null);
        Task<bool> ValidatePatientDataAsync(Patient patient);
    }
} 