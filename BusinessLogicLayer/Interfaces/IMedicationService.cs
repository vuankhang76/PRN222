using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IMedicationService
    {
        // CRUD cơ bản
        Task<IEnumerable<Medication>> GetAllMedicationsAsync();
        Task<Medication?> GetMedicationByIdAsync(int id);
        Task<Medication?> GetMedicationWithDetailsAsync(int id);
        Task<Medication> CreateMedicationAsync(Medication medication);
        Task<Medication> UpdateMedicationAsync(Medication medication);
        Task<bool> DeleteMedicationAsync(int id);

        // Business logic đặc biệt cho thuốc điều trị
        Task<IEnumerable<Medication>> GetMedicationsByTreatmentAsync(int treatmentId);
        Task<IEnumerable<Medication>> GetMedicationsByNameAsync(string medicationName);
        Task<IEnumerable<Medication>> GetMedicationsByTypeAsync(string medicationType);
        Task<IEnumerable<Medication>> GetActiveMedicationsAsync();
        Task<IEnumerable<Medication>> GetExpiredMedicationsAsync();
        Task<IEnumerable<Medication>> GetMedicationsByDosageAsync(string dosage);
        
        // Quản lý thuốc
        Task<bool> CheckMedicationInteractionAsync(int treatmentId, string newMedicationName);
        Task<IEnumerable<Medication>> GetConflictingMedicationsAsync(string medicationName);
        Task<bool> UpdateMedicationStatusAsync(int medicationId, string status);
        
        // Thống kê và báo cáo
        Task<int> GetTotalMedicationsCountAsync();
        Task<Dictionary<string, int>> GetMedicationsByTypeStatisticsAsync();
        Task<Dictionary<string, int>> GetMedicationsByStatusStatisticsAsync();
        Task<decimal> GetAverageMedicationDurationAsync();
        
        // Validation
        Task<bool> ValidateMedicationDataAsync(Medication medication);
        Task<bool> IsMedicationNameValidAsync(string medicationName);
        Task<bool> CanAddMedicationToTreatmentAsync(int treatmentId, string medicationName);
    }
} 