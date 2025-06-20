using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface ITreatmentStageService
    {
        // CRUD cơ bản
        Task<IEnumerable<TreatmentStage>> GetAllTreatmentStagesAsync();
        Task<TreatmentStage?> GetTreatmentStageByIdAsync(int id);
        Task<TreatmentStage?> GetTreatmentStageWithDetailsAsync(int id);
        Task<TreatmentStage> CreateTreatmentStageAsync(TreatmentStage treatmentStage);
        Task<TreatmentStage> UpdateTreatmentStageAsync(TreatmentStage treatmentStage);
        Task<bool> DeleteTreatmentStageAsync(int id);

        // Business logic đặc biệt cho giai đoạn điều trị
        Task<IEnumerable<TreatmentStage>> GetTreatmentStagesByTreatmentAsync(int treatmentId);
        Task<IEnumerable<TreatmentStage>> GetTreatmentStagesByStatusAsync(string status);
        Task<IEnumerable<TreatmentStage>> GetActiveTreatmentStagesAsync();
        Task<IEnumerable<TreatmentStage>> GetCompletedTreatmentStagesAsync();
        Task<TreatmentStage?> GetCurrentTreatmentStageByTreatmentAsync(int treatmentId);
        Task<TreatmentStage?> GetNextTreatmentStageAsync(int currentStageId);
        
        // Quản lý giai đoạn điều trị
        Task<bool> StartTreatmentStageAsync(int stageId);
        Task<bool> CompleteTreatmentStageAsync(int stageId, string results);
        Task<bool> MoveToNextStageAsync(int currentStageId);
        Task<bool> UpdateStageProgressAsync(int stageId, string progress);
        Task<bool> PauseTreatmentStageAsync(int stageId, string reason);
        Task<bool> ResumeTreatmentStageAsync(int stageId);
        
        // Thống kê và báo cáo
        Task<int> GetTotalTreatmentStagesCountAsync();
        Task<Dictionary<string, int>> GetTreatmentStagesByStatusStatisticsAsync();
        Task<decimal> GetAverageStageDurationAsync();
        Task<decimal> GetStageSuccessRateAsync();
        
        // Validation
        Task<bool> ValidateTreatmentStageDataAsync(TreatmentStage treatmentStage);
        Task<bool> CanCreateNewStageForTreatmentAsync(int treatmentId);
        Task<bool> IsStageOrderValidAsync(int treatmentId, int stageOrder);
    }
} 