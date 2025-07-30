using InfertilityApp.Models;

namespace InfertilityApp.DataAccessLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories cho từng entity
        IPatientRepository? Patients { get; }
        IRepository<Partner> Partners { get; }
        IRepository<Doctor> Doctors { get; }
        IRepository<Treatment> Treatments { get; }
        IRepository<TreatmentStage> TreatmentStages { get; }
        IRepository<Procedure> Procedures { get; }
        IRepository<Medication> Medications { get; }
        IRepository<MedicalRecord> MedicalRecords { get; }
        IRepository<Appointment> Appointments { get; }
        IRepository<User> Users { get; }
        IRepository<TestResult> TestResults { get; }
        IRepository<MedicationSchedule> MedicationSchedules { get; }
        IRepository<TreatmentOutcome> TreatmentOutcomes { get; }

        // Lưu thay đổi
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        
        // Truy cập DbContext để thực hiện complex queries
        ApplicationDbContext GetContext();
    }
} 