using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InfertilityApp.DataAccessLayer.Repositories
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly ApplicationDbContext _context;

        public TestResultRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestResult>> GetAllAsync()
        {
            return await _context.TestResults
                .Include(tr => tr.Patient)
                .Include(tr => tr.Partner)
                .Include(tr => tr.Doctor)
                .OrderByDescending(tr => tr.TestDate)
                .ToListAsync();
        }

        public async Task<TestResult?> GetByIdAsync(int id)
        {
            return await _context.TestResults
                .Include(tr => tr.Patient)
                .Include(tr => tr.Partner)
                .Include(tr => tr.Doctor)
                .FirstOrDefaultAsync(tr => tr.Id == id);
        }

        public async Task<TestResult> AddAsync(TestResult testResult)
        {
            _context.TestResults.Add(testResult);
            await _context.SaveChangesAsync();
            return testResult;
        }

        public async Task<TestResult> UpdateAsync(TestResult testResult)
        {
            _context.TestResults.Update(testResult);
            await _context.SaveChangesAsync();
            return testResult;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult == null)
                return false;

            _context.TestResults.Remove(testResult);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
