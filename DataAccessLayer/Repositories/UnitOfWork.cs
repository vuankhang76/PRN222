using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace InfertilityApp.DataAccessLayer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Lazy loading cho các repositories
        private IPatientRepository? _patients;
        private IRepository<Partner>? _partners;
        private IRepository<Doctor>? _doctors;
        private IRepository<Treatment>? _treatments;
        private IRepository<TreatmentStage>? _treatmentStages;
        private IRepository<Procedure>? _procedures;
        private IRepository<Medication>? _medications;
        private IRepository<MedicalRecord>? _medicalRecords;
        private IRepository<Appointment>? _appointments;
        private IRepository<User>? _users;
        private IRepository<TestResult>? _testResults;
        private IRepository<MedicationSchedule>? _medicationSchedules;
        private IRepository<TreatmentOutcome>? _treatmentOutcomes;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IPatientRepository Patients =>
    _patients ??= new PatientRepository(_context);

        public IRepository<Partner> Partners => 
            _partners ??= new Repository<Partner>(_context);

        public IRepository<Doctor> Doctors => 
            _doctors ??= new Repository<Doctor>(_context);

        public IRepository<Treatment> Treatments => 
            _treatments ??= new Repository<Treatment>(_context);

        public IRepository<TreatmentStage> TreatmentStages => 
            _treatmentStages ??= new Repository<TreatmentStage>(_context);

        public IRepository<Procedure> Procedures => 
            _procedures ??= new Repository<Procedure>(_context);

        public IRepository<Medication> Medications => 
            _medications ??= new Repository<Medication>(_context);

        public IRepository<MedicalRecord> MedicalRecords => 
            _medicalRecords ??= new Repository<MedicalRecord>(_context);

        public IRepository<Appointment> Appointments => 
            _appointments ??= new Repository<Appointment>(_context);

        public IRepository<User> Users => 
            _users ??= new Repository<User>(_context);

        public IRepository<TestResult> TestResults => 
            _testResults ??= new Repository<TestResult>(_context);

        public IRepository<MedicationSchedule> MedicationSchedules => 
            _medicationSchedules ??= new Repository<MedicationSchedule>(_context);

        public IRepository<TreatmentOutcome> TreatmentOutcomes => 
            _treatmentOutcomes ??= new Repository<TreatmentOutcome>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public ApplicationDbContext GetContext()
        {
            return _context;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 