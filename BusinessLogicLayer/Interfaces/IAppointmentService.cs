using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IAppointmentService
    {
        // CRUD cơ bản
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<Appointment?> GetAppointmentWithDetailsAsync(int id);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> DeleteAppointmentAsync(int id);

        // Business logic đặc biệt cho quản lý lịch hẹn
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetPendingAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(string status);

        // Quản lý lịch hẹn
        Task<bool> ConfirmAppointmentAsync(int appointmentId);
        Task<bool> CancelAppointmentAsync(int appointmentId, string reason);
        Task<bool> RescheduleAppointmentAsync(int appointmentId, DateTime newDateTime);
        Task<bool> CompleteAppointmentAsync(int appointmentId, string notes);
        Task<bool> CheckInPatientAsync(int appointmentId);

        // Kiểm tra và validation
        Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime appointmentDateTime);
        Task<bool> IsPatientAvailableAsync(int patientId, DateTime appointmentDateTime);
        Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime appointmentDateTime);
        Task<IEnumerable<DateTime>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date);

        // Thống kê và báo cáo
        Task<int> GetTotalAppointmentsCountAsync();
        Task<Dictionary<string, int>> GetAppointmentsByStatusStatisticsAsync();
        Task<Dictionary<DateTime, int>> GetAppointmentsByDateStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAppointmentCompletionRateAsync();
        Task<decimal> GetAppointmentCancellationRateAsync();

        // Nhắc nhở và thông báo
        Task<IEnumerable<Appointment>> GetAppointmentsNeedingReminderAsync();
        Task<bool> SendAppointmentReminderAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetTodayAppointmentsForDoctorAsync(int doctorId);
    }
} 