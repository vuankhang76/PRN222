using InfertilityApp.Models;

namespace InfertilityApp.DataAccessLayer.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<Patient?> GetPatientFullDetailsAsync(int id);
    }
}
