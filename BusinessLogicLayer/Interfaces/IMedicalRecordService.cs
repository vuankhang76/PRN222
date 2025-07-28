using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IMedicalRecordService
    {
        // CRUD cơ bản
        Task<IEnumerable<MedicalRecord>> GetAllMedicalRecordsAsync();
        Task<IEnumerable<MedicalRecord>> GetAllMedicalRecordsWithDetailsAsync();
        Task<MedicalRecord?> GetMedicalRecordByIdAsync(int id);
        Task<MedicalRecord?> GetMedicalRecordWithDetailsAsync(int id);
        Task<MedicalRecord> CreateMedicalRecordAsync(MedicalRecord medicalRecord);
        Task<MedicalRecord> UpdateMedicalRecordAsync(MedicalRecord medicalRecord);
        Task<bool> DeleteMedicalRecordAsync(int id);

        // Business logic đặc biệt cho hồ sơ y tế
        Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByPatientAsync(int patientId);
        Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByDoctorAsync(int doctorId);
        Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByRecordTypeAsync(string recordType);
        Task<MedicalRecord?> GetLatestMedicalRecordByPatientAsync(int patientId);
        
        // Tìm kiếm và lọc
        Task<IEnumerable<MedicalRecord>> SearchMedicalRecordsByDiagnosisAsync(string diagnosis);
        Task<IEnumerable<MedicalRecord>> GetCriticalMedicalRecordsAsync();
        
        // Thống kê và báo cáo
        Task<int> GetTotalMedicalRecordsCountAsync();
        Task<Dictionary<string, int>> GetMedicalRecordsByTypeStatisticsAsync();
        Task<Dictionary<DateTime, int>> GetMedicalRecordsByDateStatisticsAsync(DateTime startDate, DateTime endDate);
        
        // Validation
        Task<bool> ValidateMedicalRecordDataAsync(MedicalRecord medicalRecord);
        Task<bool> CanPatientHaveMultipleRecordsOnSameDateAsync(int patientId, DateTime recordDate);
    }
} 