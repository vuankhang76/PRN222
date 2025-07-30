using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<Medication>> GetAllMedicationsAsync()
        {
            return await _unitOfWork.Medications.GetAllAsync();
        }

        public async Task<Medication?> GetMedicationByIdAsync(int id)
        {
            return await _unitOfWork.Medications.GetByIdAsync(id);
        }

        public async Task<Medication?> GetMedicationWithDetailsAsync(int id)
        {
            return await _unitOfWork.Medications.GetByIdWithIncludeAsync(id,
                m => m.Treatment!);
        }

        public async Task<Medication> CreateMedicationAsync(Medication medication)
        {
            if (!await ValidateMedicationDataAsync(medication))
            {
                throw new ArgumentException("Dữ liệu thuốc không hợp lệ");
            }

            var treatment = await _unitOfWork.Treatments.GetByIdAsync(medication.TreatmentId);
            if (treatment == null)
            {
                throw new InvalidOperationException("Không tìm thấy điều trị");
            }

            if (treatment.Status != "Đang điều trị")
            {
                throw new InvalidOperationException("Chỉ có thể thêm thuốc cho điều trị đang tiến hành");
            }

            medication.StartDate = DateTime.Now;
            medication.Status = "Active";

            var result = await _unitOfWork.Medications.AddAsync(medication);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<Medication> UpdateMedicationAsync(Medication medication)
        {
            if (!await ValidateMedicationDataAsync(medication))
            {
                throw new ArgumentException("Dữ liệu thuốc không hợp lệ");
            }

            await _unitOfWork.Medications.UpdateAsync(medication);
            await _unitOfWork.SaveChangesAsync();
            return medication;
        }

        public async Task<bool> DeleteMedicationAsync(int id)
        {
            var medication = await _unitOfWork.Medications.GetByIdAsync(id);
            if (medication == null) return false;

            if (medication.Status == "Đang sử dụng")
            {
                throw new InvalidOperationException("Không thể xóa thuốc đang được sử dụng");
            }

            await _unitOfWork.Medications.DeleteAsync(medication);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Business logic đặc biệt
        public async Task<IEnumerable<Medication>> GetMedicationsByTreatmentAsync(int treatmentId)
        {
            return await _unitOfWork.Medications.FindAsync(m => m.TreatmentId == treatmentId);
        }

        public async Task<IEnumerable<Medication>> GetMedicationsByNameAsync(string medicationName)
        {
            return await _unitOfWork.Medications.FindAsync(m => 
                m.MedicationName != null && m.MedicationName.Contains(medicationName));
        }

        public async Task<IEnumerable<Medication>> GetMedicationsByTypeAsync(string medicationType)
        {
            return await _unitOfWork.Medications.FindAsync(m => 
                m.MedicationName != null && m.MedicationName.Contains(medicationType));
        }

        public async Task<IEnumerable<Medication>> GetActiveMedicationsAsync()
        {
            return await _unitOfWork.Medications.FindAsync(m => m.Status == "Active");
        }

        public async Task<IEnumerable<Medication>> GetExpiredMedicationsAsync()
        {
            var today = DateTime.Now;
            return await _unitOfWork.Medications.FindAsync(m => 
                m.EndDate.HasValue && m.EndDate.Value < today);
        }

        public async Task<IEnumerable<Medication>> GetMedicationsByDosageAsync(string dosage)
        {
            return await _unitOfWork.Medications.FindAsync(m => 
                m.Dosage != null && m.Dosage.Contains(dosage));
        }

        // Quản lý thuốc
        public async Task<bool> CheckMedicationInteractionAsync(int treatmentId, string newMedicationName)
        {
            var existingMedications = await GetMedicationsByTreatmentAsync(treatmentId);
            var activeMedications = existingMedications.Where(m => m.Status == "Đang sử dụng");

            // Danh sách thuốc không nên dùng chung (simplified)
            var conflictingPairs = new Dictionary<string, List<string>>
            {
                { "Clomiphene", new List<string> { "Letrozole", "Gonadotropins" } },
                { "Metformin", new List<string> { "Insulin" } },
                { "Progesterone", new List<string> { "Anti-progesterone" } }
            };

            foreach (var medication in activeMedications)
            {
                if (conflictingPairs.ContainsKey(newMedicationName) && 
                    conflictingPairs[newMedicationName].Contains(medication.MedicationName!))
                {
                    return false; // Có xung đột
                }

                if (conflictingPairs.ContainsKey(medication.MedicationName!) && 
                    conflictingPairs[medication.MedicationName!].Contains(newMedicationName))
                {
                    return false; // Có xung đột
                }
            }

            return true; // Không có xung đột
        }

        public async Task<IEnumerable<Medication>> GetConflictingMedicationsAsync(string medicationName)
        {
            var allMedications = await _unitOfWork.Medications.GetAllAsync();
            var conflictingMedications = new List<Medication>();

            foreach (var medication in allMedications)
            {
                if (!await CheckMedicationInteractionAsync(medication.TreatmentId, medicationName))
                {
                    conflictingMedications.Add(medication);
                }
            }

            return conflictingMedications;
        }

        public async Task<bool> UpdateMedicationStatusAsync(int medicationId, string status)
        {
            var medication = await _unitOfWork.Medications.GetByIdAsync(medicationId);
            if (medication == null) return false;

            medication.Status = status;
            if (status == "Đã hoàn thành" && !medication.EndDate.HasValue)
            {
                medication.EndDate = DateTime.Now;
            }

            await _unitOfWork.Medications.UpdateAsync(medication);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Thống kê và báo cáo
        public async Task<int> GetTotalMedicationsCountAsync()
        {
            return await _unitOfWork.Medications.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetMedicationsByTypeStatisticsAsync()
        {
            var allMedications = await _unitOfWork.Medications.GetAllAsync();
            return allMedications.GroupBy(m => m.MedicationName?.Split(' ')[0] ?? "Không xác định")
                               .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetMedicationsByStatusStatisticsAsync()
        {
            var allMedications = await _unitOfWork.Medications.GetAllAsync();
            return allMedications.GroupBy(m => m.Status ?? "Không xác định")
                               .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetAverageMedicationDurationAsync()
        {
            var completedMedications = await _unitOfWork.Medications.FindAsync(m => 
                m.Status == "Completed" && m.EndDate.HasValue);

            if (!completedMedications.Any()) return 0;

            var durations = completedMedications.Select(m => 
                (m.EndDate!.Value - m.StartDate).TotalDays);

            return (decimal)durations.Average();
        }

        // Validation
        public async Task<bool> ValidateMedicationDataAsync(Medication medication)
        {
            // Validate treatment exists
            var treatment = await _unitOfWork.Treatments.GetByIdAsync(medication.TreatmentId);
            if (treatment == null) return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(medication.MedicationName) ||
                string.IsNullOrWhiteSpace(medication.Dosage))
                return false;

            // Validate dates
            if (medication.EndDate.HasValue && medication.EndDate.Value < medication.StartDate)
                return false;

            return true;
        }

        public async Task<bool> IsMedicationNameValidAsync(string medicationName)
        {
            // Danh sách thuốc được phép sử dụng trong điều trị hiếm muộn
            var allowedMedications = new List<string>
            {
                "Clomiphene", "Letrozole", "Gonadotropins", "Metformin", 
                "Progesterone", "Estrogen", "GnRH agonists", "GnRH antagonists",
                "hCG", "FSH", "LH", "Bromocriptine", "Cabergoline"
            };

            return await Task.FromResult(
                allowedMedications.Any(allowed => 
                    medicationName.Contains(allowed, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<bool> CanAddMedicationToTreatmentAsync(int treatmentId, string medicationName)
        {
            // Kiểm tra tên thuốc hợp lệ
            if (!await IsMedicationNameValidAsync(medicationName))
                return false;

            // Kiểm tra xung đột thuốc
            return await CheckMedicationInteractionAsync(treatmentId, medicationName);
        }
    }
} 