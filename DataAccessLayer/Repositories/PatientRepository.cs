using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InfertilityApp.DataAccessLayer.Repositories
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Patient?> GetPatientFullDetailsAsync(int id)
        {
            return await _context.Patients
                .Include(p => p.Partner)
                .Include(p => p.Treatments)
                    .ThenInclude(t => t.Doctor)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .Include(p => p.MedicalRecords)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
