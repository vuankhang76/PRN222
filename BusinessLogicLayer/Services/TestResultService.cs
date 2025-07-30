using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepository _testResultRepository;

        public TestResultService(ITestResultRepository testResultRepository)
        {
            _testResultRepository = testResultRepository;
        }

        public async Task<IEnumerable<TestResult>> GetAllAsync()
        {
            return await _testResultRepository.GetAllAsync();
        }

        public async Task<TestResult?> GetByIdAsync(int id)
        {
            return await _testResultRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<TestResult>> GetByPatientIdAsync(int patientId)
        {
            var allTestResults = await _testResultRepository.GetAllAsync();
            return allTestResults.Where(tr => tr.PatientId == patientId);
        }

        public async Task<IEnumerable<TestResult>> GetByTreatmentIdAsync(int treatmentId)
        {
            // Note: TestResult doesn't have direct TreatmentId relationship
            // This method might need to be implemented based on business logic
            // For now, returning empty collection
            return new List<TestResult>();
        }

        public async Task<TestResult> AddAsync(TestResult testResult)
        {
            if (testResult == null)
                throw new ArgumentNullException(nameof(testResult));

            // Set creation date
            testResult.CreatedAt = DateTime.Now;

            return await _testResultRepository.AddAsync(testResult);
        }

        public async Task<TestResult> UpdateAsync(TestResult testResult)
        {
            if (testResult == null)
                throw new ArgumentNullException(nameof(testResult));

            var existingTestResult = await _testResultRepository.GetByIdAsync(testResult.Id);
            if (existingTestResult == null)
                throw new InvalidOperationException("Kết quả xét nghiệm không tồn tại.");

            return await _testResultRepository.UpdateAsync(testResult);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var testResult = await _testResultRepository.GetByIdAsync(id);
            if (testResult == null)
                return false;

            return await _testResultRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<TestResult>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var allTestResults = await _testResultRepository.GetAllAsync();
            return allTestResults.Where(tr => 
                tr.TestName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                tr.TestType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                tr.Results.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (tr.Notes != null && tr.Notes.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        public async Task<IEnumerable<TestResult>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var allTestResults = await _testResultRepository.GetAllAsync();
            return allTestResults.Where(tr => 
                tr.TestDate.Date >= fromDate.Date && 
                tr.TestDate.Date <= toDate.Date
            );
        }
    }
}
