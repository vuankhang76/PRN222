using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _unitOfWork.Appointments.GetAllAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await _unitOfWork.Appointments.GetByIdAsync(id);
        }

        public async Task<Appointment?> GetAppointmentWithDetailsAsync(int id)
        {
            return await _unitOfWork.Appointments.GetByIdWithIncludeAsync(id,
                a => a.Patient!,
                a => a.Doctor!,
                a => a.Treatment!);
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            var appointmentDateTime = appointment.AppointmentDate.Add(appointment.AppointmentTime);
            if (!await IsDoctorAvailableAsync(appointment.DoctorId, appointmentDateTime))
            {
                throw new InvalidOperationException("Bác sĩ không có lịch trống vào thời gian này");
            }

            appointment.Status = "Đã đặt";

            var result = await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null) return false;

            await _unitOfWork.Appointments.DeleteAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _unitOfWork.Appointments.GetWithIncludeAsync(
                a => a.Patient!,
                a => a.Doctor!)
                .ContinueWith(t => t.Result.Where(a => a.PatientId == patientId));
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _unitOfWork.Appointments.FindAsync(a => a.DoctorId == doctorId);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            return await _unitOfWork.Appointments.FindAsync(a => 
                a.AppointmentDate.Date == date.Date);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.Appointments.FindAsync(a => 
                a.AppointmentDate >= startDate.Date && a.AppointmentDate <= endDate.Date);
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync()
        {
            var today = DateTime.Now.Date;
            return await _unitOfWork.Appointments.FindAsync(a => a.AppointmentDate >= today);
        }

        public async Task<IEnumerable<Appointment>> GetPendingAppointmentsAsync()
        {
            return await _unitOfWork.Appointments.FindAsync(a => a.Status == "Đã đặt");
        }

        public async Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync()
        {
            return await _unitOfWork.Appointments.FindAsync(a => a.Status == "Đã hoàn thành");
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(string status)
        {
            return await _unitOfWork.Appointments.FindAsync(a => a.Status == status);
        }

        public async Task<bool> ConfirmAppointmentAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = "Đã xác nhận";
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string reason)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = "Đã hủy";
            appointment.Notes = appointment.Notes + $"\nLý do hủy: {reason}";
            
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RescheduleAppointmentAsync(int appointmentId, DateTime newDateTime)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.AppointmentDate = newDateTime.Date;
            appointment.AppointmentTime = newDateTime.TimeOfDay;
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAppointmentAsync(int appointmentId, string notes)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = "Đã hoàn thành";
            appointment.Notes = notes;
            
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckInPatientAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = "Đã check-in";
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime appointmentDateTime)
        {
            var existingAppointments = await _unitOfWork.Appointments.FindAsync(a => 
                a.DoctorId == doctorId && 
                a.AppointmentDate.Date == appointmentDateTime.Date);

            return !existingAppointments.Any(a => 
            {
                var existingDateTime = a.AppointmentDate.Add(a.AppointmentTime);
                return Math.Abs((existingDateTime - appointmentDateTime).TotalMinutes) < 60;
            });
        }

        public async Task<bool> IsPatientAvailableAsync(int patientId, DateTime appointmentDateTime)
        {
            var existingAppointments = await _unitOfWork.Appointments.FindAsync(a => 
                a.PatientId == patientId && 
                a.AppointmentDate.Date == appointmentDateTime.Date);

            return !existingAppointments.Any(a => 
            {
                var existingDateTime = a.AppointmentDate.Add(a.AppointmentTime);
                return Math.Abs((existingDateTime - appointmentDateTime).TotalMinutes) < 60;
            });
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime appointmentDateTime)
        {
            return await IsDoctorAvailableAsync(doctorId, appointmentDateTime);
        }

        public async Task<IEnumerable<DateTime>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
        {
            var slots = new List<DateTime>();
            var startTime = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0);
            var endTime = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0);

            for (var time = startTime; time <= endTime; time = time.AddMinutes(60))
            {
                if (await IsDoctorAvailableAsync(doctorId, time))
                {
                    slots.Add(time);
                }
            }

            return slots;
        }

        public async Task<int> GetTotalAppointmentsCountAsync()
        {
            return await _unitOfWork.Appointments.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetAppointmentsByStatusStatisticsAsync()
        {
            var allAppointments = await _unitOfWork.Appointments.GetAllAsync();
            return allAppointments.GroupBy(a => a.Status ?? "Không xác định")
                                 .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<DateTime, int>> GetAppointmentsByDateStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var appointments = await GetAppointmentsByDateRangeAsync(startDate, endDate);
            return appointments.GroupBy(a => a.AppointmentDate.Date)
                              .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetAppointmentCompletionRateAsync()
        {
            var totalAppointments = await _unitOfWork.Appointments.CountAsync();
            var completedAppointments = await _unitOfWork.Appointments.CountAsync(a => a.Status == "Đã hoàn thành");
            
            if (totalAppointments == 0) return 0;
            return (decimal)completedAppointments / totalAppointments * 100;
        }

        public async Task<decimal> GetAppointmentCancellationRateAsync()
        {
            var totalAppointments = await _unitOfWork.Appointments.CountAsync();
            var cancelledAppointments = await _unitOfWork.Appointments.CountAsync(a => a.Status == "Đã hủy");
            
            if (totalAppointments == 0) return 0;
            return (decimal)cancelledAppointments / totalAppointments * 100;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsNeedingReminderAsync()
        {
            var tomorrow = DateTime.Now.AddDays(1);
            return await _unitOfWork.Appointments.FindAsync(a => 
                a.AppointmentDate.Date == tomorrow.Date);
        }

        public async Task<bool> SendAppointmentReminderAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Notes = appointment.Notes + $"\nĐã gửi nhắc nhở: {DateTime.Now}";
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Appointment>> GetTodayAppointmentsForDoctorAsync(int doctorId)
        {
            var today = DateTime.Now.Date;
            return await _unitOfWork.Appointments.FindAsync(a => 
                a.DoctorId == doctorId && 
                a.AppointmentDate.Date == today);
        }
    }
} 