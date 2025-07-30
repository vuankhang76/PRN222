using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface ITestResultService
    {
        Task<IEnumerable<TestResult>> GetAllAsync();
        Task<TestResult?> GetByIdAsync(int id);
        Task<IEnumerable<TestResult>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<TestResult>> GetByTreatmentIdAsync(int treatmentId);
        Task<TestResult> AddAsync(TestResult testResult);
        Task<TestResult> UpdateAsync(TestResult testResult);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TestResult>> SearchAsync(string searchTerm);
        Task<IEnumerable<TestResult>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
