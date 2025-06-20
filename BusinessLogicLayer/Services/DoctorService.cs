using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
        {
            return await _unitOfWork.Doctors.GetAllAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            return await _unitOfWork.Doctors.GetByIdAsync(id);
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            var result = await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<Doctor> UpdateDoctorAsync(Doctor doctor)
        {
            await _unitOfWork.Doctors.UpdateAsync(doctor);
            await _unitOfWork.SaveChangesAsync();
            return doctor;
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return false;

            await _unitOfWork.Doctors.DeleteAsync(doctor);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(string specialization)
        {
            return await _unitOfWork.Doctors.FindAsync(d => d.Specialization == specialization);
        }

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
        {
            // IsActive property không tồn tại trong Doctor model - trả về tất cả doctors
            return await _unitOfWork.Doctors.GetAllAsync();
        }

        public async Task<IEnumerable<Doctor>> SearchDoctorsByNameAsync(string name)
        {
            return await _unitOfWork.Doctors.FindAsync(d => d.FullName.Contains(name));
        }

        public async Task<int> GetTotalDoctorsCountAsync()
        {
            return await _unitOfWork.Doctors.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetDoctorsBySpecializationStatisticsAsync()
        {
            var allDoctors = await _unitOfWork.Doctors.GetAllAsync();
            return allDoctors.GroupBy(d => d.Specialization ?? "Không xác định")
                           .ToDictionary(g => g.Key, g => g.Count());
        }
    }
} 