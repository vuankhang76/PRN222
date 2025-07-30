using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _unitOfWork.Patients.GetAllAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _unitOfWork.Patients.GetByIdAsync(id);
        }

        public async Task<Patient?> GetPatientWithDetailsAsync(int id)
        {
            return await _unitOfWork.Patients.GetPatientFullDetailsAsync(id);
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            // Validation business rules
            if (!await ValidatePatientDataAsync(patient))
            {
                throw new ArgumentException("Dữ liệu bệnh nhân không hợp lệ");
            }

            if (!await IsEmailUniqueAsync(patient.Email))
            {
                throw new ArgumentException("Email đã tồn tại trong hệ thống");
            }

            if (!await IsPhoneNumberUniqueAsync(patient.PhoneNumber))
            {
                throw new ArgumentException("Số điện thoại đã tồn tại trong hệ thống");
            }

            patient.RegistrationDate = DateTime.Now;
            patient.CreatedAt = DateTime.Now;

            var result = await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<Patient> UpdatePatientAsync(Patient patient)
        {
            // Validation business rules
            if (!await ValidatePatientDataAsync(patient))
            {
                throw new ArgumentException("Dữ liệu bệnh nhân không hợp lệ");
            }

            if (!await IsEmailUniqueAsync(patient.Email, patient.Id))
            {
                throw new ArgumentException("Email đã tồn tại trong hệ thống");
            }

            if (!await IsPhoneNumberUniqueAsync(patient.PhoneNumber, patient.Id))
            {
                throw new ArgumentException("Số điện thoại đã tồn tại trong hệ thống");
            }

            await _unitOfWork.Patients.UpdateAsync(patient);
            await _unitOfWork.SaveChangesAsync();
            return patient;
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return false;

            // Kiểm tra business rules trước khi xóa
            var treatments = await _unitOfWork.Treatments.FindAsync(t => t.PatientId == id);
            if (treatments.Any(t => t.Status == "Đang điều trị"))
            {
                throw new InvalidOperationException("Không thể xóa bệnh nhân đang trong quá trình điều trị");
            }

            await _unitOfWork.Patients.DeleteAsync(patient);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Business logic đặc biệt
        public async Task<IEnumerable<Patient>> GetPatientsByAgeRangeAsync(int minAge, int maxAge)
        {
            var currentDate = DateTime.Now;
            var maxBirthDate = currentDate.AddYears(-minAge);
            var minBirthDate = currentDate.AddYears(-maxAge - 1);

            return await _unitOfWork.Patients.FindAsync(p => 
                p.DateOfBirth <= maxBirthDate && p.DateOfBirth >= minBirthDate);
        }

        public async Task<IEnumerable<Patient>> GetPatientsByGenderAsync(string gender)
        {
            return await _unitOfWork.Patients.FindAsync(p => p.Gender == gender);
        }

        public async Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string name)
        {
            return await _unitOfWork.Patients.FindAsync(p => 
                p.FullName.Contains(name));
        }

        public async Task<IEnumerable<Patient>> GetPatientsWithTreatmentHistoryAsync()
        {
            return await _unitOfWork.Patients.GetWithIncludeAsync(p => p.Treatments!);
        }

        public async Task<IEnumerable<Patient>> GetPatientsWithoutPartnerAsync()
        {
            return await _unitOfWork.Patients.FindAsync(p => p.Partner == null);
        }

        // Báo cáo và thống kê
        public async Task<int> GetTotalPatientsCountAsync()
        {
            return await _unitOfWork.Patients.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetPatientsByGenderStatisticsAsync()
        {
            var allPatients = await _unitOfWork.Patients.GetAllAsync();
            return allPatients.GroupBy(p => p.Gender)
                             .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<int, int>> GetPatientsByAgeGroupStatisticsAsync()
        {
            var allPatients = await _unitOfWork.Patients.GetAllAsync();
            var currentDate = DateTime.Now;
            
            return allPatients.GroupBy(p => (currentDate.Year - p.DateOfBirth.Year) / 10 * 10)
                             .ToDictionary(g => g.Key, g => g.Count());
        }

        // Validation
        public async Task<bool> IsEmailUniqueAsync(string email, int? excludePatientId = null)
        {
            var existingPatient = await _unitOfWork.Patients.GetAsync(p => p.Email == email);
            return existingPatient == null || (excludePatientId.HasValue && existingPatient.Id == excludePatientId.Value);
        }

        public async Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber, int? excludePatientId = null)
        {
            var existingPatient = await _unitOfWork.Patients.GetAsync(p => p.PhoneNumber == phoneNumber);
            return existingPatient == null || (excludePatientId.HasValue && existingPatient.Id == excludePatientId.Value);
        }

        public async Task<bool> ValidatePatientDataAsync(Patient patient)
        {
            // Validate age (18-60 for infertility treatment)
            var age = DateTime.Now.Year - patient.DateOfBirth.Year;
            if (age < 18 || age > 60)
            {
                return false;
            }

            // Validate gender
            if (patient.Gender != "Nam" && patient.Gender != "Nữ")
            {
                return false;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(patient.FullName) ||
                string.IsNullOrWhiteSpace(patient.Email) ||
                string.IsNullOrWhiteSpace(patient.PhoneNumber))
            {
                return false;
            }

            return await Task.FromResult(true);
        }
    }
} 