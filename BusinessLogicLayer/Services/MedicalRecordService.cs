using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalRecordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<MedicalRecord>> GetAllMedicalRecordsAsync()
        {
            return await _unitOfWork.MedicalRecords.GetAllAsync();
        }

        public async Task<IEnumerable<MedicalRecord>> GetAllMedicalRecordsWithDetailsAsync()
        {
            return await _unitOfWork.MedicalRecords.GetWithIncludeAsync(
                mr => mr.Patient!,
                mr => mr.Doctor!);
        }

        public async Task<MedicalRecord?> GetMedicalRecordByIdAsync(int id)
        {
            return await _unitOfWork.MedicalRecords.GetByIdAsync(id);
        }

        public async Task<MedicalRecord?> GetMedicalRecordWithDetailsAsync(int id)
        {
            return await _unitOfWork.MedicalRecords.GetByIdWithIncludeAsync(id,
                mr => mr.Patient!,
                mr => mr.Doctor!);
        }

        public async Task<MedicalRecord> CreateMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            if (!await ValidateMedicalRecordDataAsync(medicalRecord))
            {
                throw new ArgumentException("Dữ liệu hồ sơ y tế không hợp lệ");
            }

            medicalRecord.RecordDate = DateTime.Now;
            // CreatedAt property không tồn tại trong MedicalRecord model
            medicalRecord.RecordDate = DateTime.Now;

            var result = await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<MedicalRecord> UpdateMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            if (!await ValidateMedicalRecordDataAsync(medicalRecord))
            {
                throw new ArgumentException("Dữ liệu hồ sơ y tế không hợp lệ");
            }

            await _unitOfWork.MedicalRecords.UpdateAsync(medicalRecord);
            await _unitOfWork.SaveChangesAsync();
            return medicalRecord;
        }

        public async Task<bool> DeleteMedicalRecordAsync(int id)
        {
            var medicalRecord = await _unitOfWork.MedicalRecords.GetByIdAsync(id);
            if (medicalRecord == null) return false;

            await _unitOfWork.MedicalRecords.DeleteAsync(medicalRecord);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Business logic đặc biệt
        public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByPatientAsync(int patientId)
        {
            return await _unitOfWork.MedicalRecords.FindAsync(mr => mr.PatientId == patientId);
        }

        public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByDoctorAsync(int doctorId)
        {
            return await _unitOfWork.MedicalRecords.FindAsync(mr => mr.DoctorId == doctorId);
        }

        public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.MedicalRecords.FindAsync(mr => 
                mr.RecordDate >= startDate && mr.RecordDate <= endDate);
        }

        public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByRecordTypeAsync(string recordType)
        {
            return await _unitOfWork.MedicalRecords.FindAsync(mr => mr.RecordType == recordType);
        }

        public async Task<MedicalRecord?> GetLatestMedicalRecordByPatientAsync(int patientId)
        {
            var records = await _unitOfWork.MedicalRecords.FindAsync(mr => mr.PatientId == patientId);
            return records.OrderByDescending(mr => mr.RecordDate).FirstOrDefault();
        }

        public async Task<IEnumerable<MedicalRecord>> SearchMedicalRecordsByDiagnosisAsync(string diagnosis)
        {
            return await _unitOfWork.MedicalRecords.FindAsync(mr => 
                mr.Diagnosis != null && mr.Diagnosis.Contains(diagnosis));
        }

        public async Task<IEnumerable<MedicalRecord>> GetCriticalMedicalRecordsAsync()
        {
            return await _unitOfWork.MedicalRecords.FindAsync(mr => 
                mr.Diagnosis != null && (
                    mr.Diagnosis.Contains("nghiêm trọng") ||
                    mr.Diagnosis.Contains("cấp cứu") ||
                    mr.Diagnosis.Contains("khẩn cấp")
                ));
        }

        public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByPatientIdAsync(string patientId)
        {
            // First get all records with includes
            var records = await _unitOfWork.MedicalRecords.GetWithIncludeAsync(
                mr => mr.Patient!,
                mr => mr.Doctor!);
            
            // Then filter in memory
            return records.Where(mr => mr.PatientId.ToString() == patientId);
        }

        // Thống kê và báo cáo
        public async Task<int> GetTotalMedicalRecordsCountAsync()
        {
            return await _unitOfWork.MedicalRecords.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetMedicalRecordsByTypeStatisticsAsync()
        {
            var allRecords = await _unitOfWork.MedicalRecords.GetAllAsync();
            return allRecords.GroupBy(mr => mr.RecordType ?? "Không xác định")
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<DateTime, int>> GetMedicalRecordsByDateStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var records = await GetMedicalRecordsByDateRangeAsync(startDate, endDate);
            return records.GroupBy(mr => mr.RecordDate.Date)
                         .ToDictionary(g => g.Key, g => g.Count());
        }

        // Validation
        public async Task<bool> ValidateMedicalRecordDataAsync(MedicalRecord medicalRecord)
        {
            // Validate patient exists
            var patient = await _unitOfWork.Patients.GetByIdAsync(medicalRecord.PatientId);
            if (patient == null) return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(medicalRecord.RecordType))
                return false;

            // Validate record date is not in future
            if (medicalRecord.RecordDate > DateTime.Now)
                return false;

            return true;
        }

        public async Task<bool> CanPatientHaveMultipleRecordsOnSameDateAsync(int patientId, DateTime recordDate)
        {
            var existingRecords = await _unitOfWork.MedicalRecords.FindAsync(mr => 
                mr.PatientId == patientId && mr.RecordDate.Date == recordDate.Date);
            
            // Cho phép tối đa 3 hồ sơ trong 1 ngày
            return existingRecords.Count() < 3;
        }
    }
} 