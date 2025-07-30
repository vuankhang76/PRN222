using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IPartnerService
    {
        // CRUD cơ bản
        Task<IEnumerable<Partner>> GetAllPartnersAsync();
        Task<Partner?> GetPartnerByIdAsync(int id);
        Task<Partner?> GetPartnerWithDetailsAsync(int id);
        Task<Partner> CreatePartnerAsync(Partner partner);
        Task<Partner> UpdatePartnerAsync(Partner partner);
        Task<bool> DeletePartnerAsync(int id);

        // Business logic đặc biệt cho người phối mẫu
        Task<Partner?> GetPartnerByPatientIdAsync(int patientId);
        Task<IEnumerable<Partner>> GetPartnersByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<Partner>> GetPartnersByGenderAsync(string gender);
        Task<IEnumerable<Partner>> GetPartnersWithMedicalHistoryAsync();
        
        // Validation và kiểm tra
        Task<bool> ValidatePartnerDataAsync(Partner partner);
        Task<bool> CanPatientHavePartnerAsync(int patientId);
        Task<bool> IsPartnerInfoCompleteAsync(int partnerId);
        
        // Thống kê
        Task<int> GetTotalPartnersCountAsync();
        Task<Dictionary<string, int>> GetPartnersByGenderStatisticsAsync();
        Task<decimal> GetAveragePartnerAgeAsync();
    }
} 