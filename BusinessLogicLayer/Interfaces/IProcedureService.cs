using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IProcedureService
    {
        // CRUD cơ bản
        Task<IEnumerable<Procedure>> GetAllProceduresAsync();
        Task<Procedure?> GetProcedureByIdAsync(int id);
        Task<Procedure?> GetProcedureWithDetailsAsync(int id);
        Task<Procedure> CreateProcedureAsync(Procedure procedure);
        Task<Procedure> UpdateProcedureAsync(Procedure procedure);
        Task<bool> DeleteProcedureAsync(int id);

        // Business logic đặc biệt cho thủ thuật
        Task<IEnumerable<Procedure>> GetProceduresByTreatmentStageAsync(int treatmentStageId);
        Task<IEnumerable<Procedure>> GetProceduresByTypeAsync(string procedureType);
        Task<IEnumerable<Procedure>> GetProceduresByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Procedure>> GetCompletedProceduresAsync();
        Task<IEnumerable<Procedure>> GetPendingProceduresAsync();
        Task<IEnumerable<Procedure>> GetProceduresByStatusAsync(string status);
        
        // Quản lý thủ thuật
        Task<bool> StartProcedureAsync(int procedureId);
        Task<bool> CompleteProcedureAsync(int procedureId, string results);
        Task<bool> CancelProcedureAsync(int procedureId, string reason);
        Task<bool> UpdateProcedureStatusAsync(int procedureId, string status);
        
        // Thống kê và báo cáo
        Task<int> GetTotalProceduresCountAsync();
        Task<Dictionary<string, int>> GetProceduresByTypeStatisticsAsync();
        Task<Dictionary<string, int>> GetProceduresByStatusStatisticsAsync();
        Task<decimal> GetProcedureSuccessRateAsync();
        Task<decimal> GetAverageProcedureDurationAsync();
        
        // Validation
        Task<bool> ValidateProcedureDataAsync(Procedure procedure);
        Task<bool> CanScheduleProcedureAsync(int treatmentStageId, DateTime scheduledDate);
        Task<bool> IsProcedureTypeValidForTreatmentAsync(string procedureType, int treatmentStageId);
    }
} 