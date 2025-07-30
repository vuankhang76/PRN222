using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InfertilityApp.Models;
using InfertilityApp.BusinessLogicLayer.Interfaces;

namespace InfertilityApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public AppointmentsController(IAppointmentService appointmentService, IPatientService patientService, IDoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;
        }

        // GET: Appointments
        public async Task<IActionResult> Index(DateTime? date, int? doctorId, int? patientId, string status)
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            // Lọc theo ngày
            if (date.HasValue)
            {
                appointments = appointments.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }

            // Lọc theo bác sĩ
            if (doctorId.HasValue)
            {
                appointments = appointments.Where(a => a.DoctorId == doctorId.Value);
            }

            // Lọc theo bệnh nhân
            if (patientId.HasValue)
            {
                appointments = appointments.Where(a => a.PatientId == patientId.Value);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                appointments = appointments.Where(a => a.Status == status);
            }

            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();

            ViewData["Patients"] = new SelectList(patients, "Id", "FullName", patientId);
            ViewData["Doctors"] = new SelectList(doctors, "Id", "FullName", doctorId);
            ViewData["Statuses"] = new SelectList(new[] { "Đã đặt", "Đã xác nhận", "Hoàn thành", "Đã hủy", "Không đến" }, status);
            ViewData["SelectedDate"] = date?.ToString("yyyy-MM-dd");

            return View(appointments.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime));
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetAppointmentWithDetailsAsync(id.Value);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create(int? patientId, int? doctorId, DateTime? date)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();

            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", doctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", patientId);

            var appointment = new Appointment
            {
                AppointmentDate = date ?? DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(9, 0, 0),
                Status = "Đã lên lịch",
                PatientId = patientId ?? 0,
                DoctorId = doctorId ?? 0
            };

            return View(appointment);
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,AppointmentDate,AppointmentTime,AppointmentType,Duration,Purpose,Status,Notes")] Appointment appointment)
        {
            var appointmentDateTime = appointment.AppointmentDate.Add(appointment.AppointmentTime);

            // ✅ Kiểm tra thời gian trong quá khứ
            if (appointmentDateTime < DateTime.Now)
            {
                ModelState.AddModelError("", "Không thể đặt lịch trong quá khứ.");
            }

            // ✅ Kiểm tra ngoài khung giờ 08:00 - 17:00
            var startHour = new TimeSpan(8, 0, 0);
            var endHour = new TimeSpan(17, 0, 0);
            if (appointment.AppointmentTime < startHour || appointment.AppointmentTime > endHour)
            {
                ModelState.AddModelError("", "Thời gian hẹn phải trong khoảng từ 08:00 đến 17:00.");
            }

            // ✅ Kiểm tra thời lượng hợp lệ (15–60 phút)
            var minDuration = TimeSpan.FromMinutes(15);
            var maxDuration = TimeSpan.FromMinutes(60);
            if (appointment.Duration < 15 || appointment.Duration > 60)
            {
                ModelState.AddModelError(nameof(appointment.Duration), "Thời lượng hẹn phải từ 15 đến 60 phút.");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Kiểm tra bác sĩ trống lịch
                    if (!await _appointmentService.IsDoctorAvailableAsync(appointment.DoctorId, appointmentDateTime))
                    {
                        ModelState.AddModelError("", "Bác sĩ không có sẵn vào thời gian này.");
                    }
                    // ✅ Kiểm tra bệnh nhân trống lịch
                    else if (!await _appointmentService.IsPatientAvailableAsync(appointment.PatientId, appointmentDateTime))
                    {
                        ModelState.AddModelError("", "Bệnh nhân đã có lịch hẹn khác vào thời gian này.");
                    }
                    else
                    {
                        await _appointmentService.CreateAppointmentAsync(appointment);
                        TempData["Success"] = "Lịch hẹn đã được tạo thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            // ✅ Load lại dropdown khi có lỗi
            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", appointment.PatientId);
            return View(appointment);
        }



        // GET: Appointments/Edit/5
        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id.Value);
            if (appointment == null)
            {
                return NotFound();
            }

            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", appointment.PatientId);
            return View(appointment);
        }


        // POST: Appointments/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,AppointmentDate,AppointmentTime,AppointmentType,Duration,Purpose,Status,Notes")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            var appointmentDateTime = appointment.AppointmentDate.Add(appointment.AppointmentTime);

            // ✅ Kiểm tra thời gian trong quá khứ
            if (appointmentDateTime < DateTime.Now)
            {
                ModelState.AddModelError("", "Không thể đặt lịch trong quá khứ.");
            }

            // ✅ Kiểm tra ngoài khung giờ 08:00 - 17:00
            var startHour = new TimeSpan(8, 0, 0);
            var endHour = new TimeSpan(17, 0, 0);
            if (appointment.AppointmentTime < startHour || appointment.AppointmentTime > endHour)
            {
                ModelState.AddModelError("", "Thời gian hẹn phải trong khoảng từ 08:00 đến 17:00.");
            }

            // ✅ Kiểm tra thời lượng hợp lệ
            if (appointment.Duration < 15 || appointment.Duration > 60)
            {
                ModelState.AddModelError(nameof(appointment.Duration), "Thời lượng hẹn phải từ 15 đến 60 phút.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Kiểm tra bác sĩ có trống (bỏ qua lịch hiện tại)
                    if (!await _appointmentService.IsDoctorAvailableAsync(appointment.DoctorId, appointmentDateTime, appointment.Id))
                    {
                        ModelState.AddModelError("", "Bác sĩ không có sẵn vào thời gian này.");
                    }
                    // ✅ Kiểm tra bệnh nhân có trống (bỏ qua lịch hiện tại)
                    else if (!await _appointmentService.IsPatientAvailableAsync(appointment.PatientId, appointmentDateTime, appointment.Id))
                    {
                        ModelState.AddModelError("", "Bệnh nhân đã có lịch hẹn khác vào thời gian này.");
                    }
                    else
                    {
                        await _appointmentService.UpdateAppointmentAsync(appointment);
                        TempData["Success"] = "Lịch hẹn đã được cập nhật thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật lịch hẹn: {ex.Message}");
                }
            }

            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", appointment.PatientId);
            return View(appointment);
        }


        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetAppointmentWithDetailsAsync(id.Value);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _appointmentService.DeleteAppointmentAsync(id);
                TempData["Success"] = "Lịch hẹn đã được xóa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa lịch hẹn: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Action methods cho quản lý lịch hẹn
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            try
            {
                await _appointmentService.ConfirmAppointmentAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int id, string reason)
        {
            try
            {
                await _appointmentService.CancelAppointmentAsync(id, reason);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RescheduleAppointment(int id, DateTime newDate, TimeSpan newTime)
        {
            try
            {
                var newDateTime = newDate.Add(newTime);
                await _appointmentService.RescheduleAppointmentAsync(id, newDateTime);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Appointments/GetAvailableSlots
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            try
            {
                var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, date);
                return Json(availableSlots);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: Appointments/Calendar
        public async Task<IActionResult> Calendar(int? doctorId, DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var appointments = await _appointmentService.GetUpcomingAppointmentsAsync();

            // Lọc theo ngày được chọn
            appointments = appointments.Where(a => 
                a.AppointmentDate >= selectedDate && 
                a.AppointmentDate <= selectedDate.AddDays(30));

            if (doctorId.HasValue)
            {
                appointments = appointments.Where(a => a.DoctorId == doctorId.Value);
            }

            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["Doctors"] = new SelectList(doctors, "Id", "FullName", doctorId);
            ViewData["SelectedDate"] = selectedDate;

            return View(appointments);
        }

        // GET: Appointments/Today
        public async Task<IActionResult> Today()
        {
            var today = DateTime.Today;
            var appointments = await _appointmentService.GetAppointmentsByDateAsync(today);

            ViewBag.TotalAppointments = appointments.Count();
            ViewBag.ConfirmedAppointments = appointments.Count(a => a.Status == "Đã xác nhận");
            ViewBag.CompletedAppointments = appointments.Count(a => a.Status == "Hoàn thành");
            ViewBag.CancelledAppointments = appointments.Count(a => a.Status == "Đã hủy");

            return View(appointments.OrderBy(a => a.AppointmentTime));
        }

        // GET: Appointments/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalAppointments = await _appointmentService.GetTotalAppointmentsCountAsync();
            var appointmentsByStatus = await _appointmentService.GetAppointmentsByStatusStatisticsAsync();
            var completionRate = await _appointmentService.GetAppointmentCompletionRateAsync();
            var cancellationRate = await _appointmentService.GetAppointmentCancellationRateAsync();

            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.AppointmentsByStatus = appointmentsByStatus;
            ViewBag.CompletionRate = completionRate;
            ViewBag.CancellationRate = cancellationRate;

            return View();
        }

        // Action cho tính năng chart thống kê
        public async Task<IActionResult> GetAppointmentStatistics()
        {
            var statistics = new
            {
                statusStats = await _appointmentService.GetAppointmentsByStatusStatisticsAsync(),
                totalCount = await _appointmentService.GetTotalAppointmentsCountAsync(),
                completionRate = await _appointmentService.GetAppointmentCompletionRateAsync(),
                cancellationRate = await _appointmentService.GetAppointmentCancellationRateAsync()
            };

            return Json(statistics);
        }
    }
} 