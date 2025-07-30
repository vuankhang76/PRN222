using InfertilityApp.Models;

namespace InfertilityApp.DataAccessLayer.Interfaces
{
    public interface ITestResultRepository
    {
        Task<IEnumerable<TestResult>> GetAllAsync();
        Task<TestResult?> GetByIdAsync(int id);
        Task<TestResult> AddAsync(TestResult testResult);
        Task<TestResult> UpdateAsync(TestResult testResult);
        Task<bool> DeleteAsync(int id);
    }
}
