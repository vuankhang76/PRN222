using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PartnerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<Partner>> GetAllPartnersAsync()
        {
            return await _unitOfWork.Partners.GetAllAsync();
        }

        public async Task<Partner?> GetPartnerByIdAsync(int id)
        {
            return await _unitOfWork.Partners.GetByIdAsync(id);
        }

        public async Task<Partner?> GetPartnerWithDetailsAsync(int id)
        {
            return await _unitOfWork.Partners.GetByIdWithIncludeAsync(id,
                p => p.Patient!);
        }

        public async Task<Partner> CreatePartnerAsync(Partner partner)
        {
            if (!await ValidatePartnerDataAsync(partner))
            {
                throw new ArgumentException("Dữ liệu người phối mẫu không hợp lệ");
            }

            if (!await CanPatientHavePartnerAsync(partner.PatientId))
            {
                throw new InvalidOperationException("Bệnh nhân đã có thông tin người phối mẫu");
            }

            var result = await _unitOfWork.Partners.AddAsync(partner);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<Partner> UpdatePartnerAsync(Partner partner)
        {
            if (!await ValidatePartnerDataAsync(partner))
            {
                throw new ArgumentException("Dữ liệu người phối mẫu không hợp lệ");
            }

            await _unitOfWork.Partners.UpdateAsync(partner);
            await _unitOfWork.SaveChangesAsync();
            return partner;
        }

        public async Task<bool> DeletePartnerAsync(int id)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            if (partner == null) return false;

            await _unitOfWork.Partners.DeleteAsync(partner);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Business logic đặc biệt
        public async Task<Partner?> GetPartnerByPatientIdAsync(int patientId)
        {
            return await _unitOfWork.Partners.GetAsync(p => p.PatientId == patientId);
        }

        public async Task<IEnumerable<Partner>> GetPartnersByAgeRangeAsync(int minAge, int maxAge)
        {
            var currentDate = DateTime.Now;
            var maxBirthDate = currentDate.AddYears(-minAge);
            var minBirthDate = currentDate.AddYears(-maxAge - 1);

            return await _unitOfWork.Partners.FindAsync(p => 
                p.DateOfBirth <= maxBirthDate && p.DateOfBirth >= minBirthDate);
        }

        public async Task<IEnumerable<Partner>> GetPartnersByGenderAsync(string gender)
        {
            return await _unitOfWork.Partners.FindAsync(p => p.Gender == gender);
        }

        public async Task<IEnumerable<Partner>> GetPartnersWithMedicalHistoryAsync()
        {
            return await _unitOfWork.Partners.FindAsync(p => 
                !string.IsNullOrEmpty(p.MedicalHistory));
        }

        // Validation và kiểm tra
        public async Task<bool> ValidatePartnerDataAsync(Partner partner)
        {
            // Validate patient exists
            var patient = await _unitOfWork.Patients.GetByIdAsync(partner.PatientId);
            if (patient == null) return false;

            // Validate age (18-70 for partner)
            var age = DateTime.Now.Year - partner.DateOfBirth.Year;
            if (age < 18 || age > 70)
                return false;

            // Validate gender
            if (partner.Gender != "Nam" && partner.Gender != "Nữ")
                return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(partner.FullName))
                return false;

            return true;
        }

        public async Task<bool> CanPatientHavePartnerAsync(int patientId)
        {
            var existingPartner = await GetPartnerByPatientIdAsync(patientId);
            return existingPartner == null;
        }

        public async Task<bool> IsPartnerInfoCompleteAsync(int partnerId)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(partnerId);
            if (partner == null) return false;

            return !string.IsNullOrWhiteSpace(partner.FullName) &&
                   !string.IsNullOrWhiteSpace(partner.PhoneNumber) &&
                   !string.IsNullOrWhiteSpace(partner.Occupation) &&
                   partner.DateOfBirth != default;
        }

        // Thống kê
        public async Task<int> GetTotalPartnersCountAsync()
        {
            return await _unitOfWork.Partners.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetPartnersByGenderStatisticsAsync()
        {
            var allPartners = await _unitOfWork.Partners.GetAllAsync();
            return allPartners.GroupBy(p => p.Gender ?? "Không xác định")
                             .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetAveragePartnerAgeAsync()
        {
            var allPartners = await _unitOfWork.Partners.GetAllAsync();
            if (!allPartners.Any()) return 0;

            var currentDate = DateTime.Now;
            var ages = allPartners.Select(p => currentDate.Year - p.DateOfBirth.Year);
            return (decimal)ages.Average();
        }
    }
} 