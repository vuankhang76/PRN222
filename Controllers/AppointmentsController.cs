using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InfertilityApp.Models;

namespace InfertilityApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index(string searchString, string status, DateTime? fromDate, DateTime? toDate, int? doctorId)
        {
            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Treatment)
                .AsQueryable();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                appointments = appointments.Where(a => 
                    a.Patient.FullName.Contains(searchString) || 
                    a.Doctor.FullName.Contains(searchString) ||
                    a.Notes.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                appointments = appointments.Where(a => a.Status == status);
            }

            // Lọc theo ngày
            if (fromDate.HasValue)
            {
                appointments = appointments.Where(a => a.AppointmentDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                appointments = appointments.Where(a => a.AppointmentDate <= toDate.Value);
            }

            // Lọc theo bác sĩ
            if (doctorId.HasValue)
            {
                appointments = appointments.Where(a => a.DoctorId == doctorId.Value);
            }

            // Chuẩn bị dữ liệu cho dropdown
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName");
            ViewData["StatusList"] = new List<string> { "Scheduled", "Completed", "Cancelled", "No-show" };

            // Lưu các tham số lọc để hiển thị lại khi refresh trang
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentDoctorId"] = doctorId;

            return View(await appointments.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime).ToListAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Treatment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create(int? patientId, int? treatmentId)
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName");
            
            // Nếu có patientId, lọc bác sĩ theo bác sĩ đang điều trị cho bệnh nhân
            if (patientId.HasValue)
            {
                var doctorIds = _context.Treatments
                    .Where(t => t.PatientId == patientId && t.Status == "Active")
                    .Select(t => t.DoctorId)
                    .Distinct()
                    .ToList();
                
                if (doctorIds.Any())
                {
                    ViewData["DoctorId"] = new SelectList(_context.Doctors.Where(d => doctorIds.Contains(d.Id)), "Id", "FullName");
                }
            }
            
            // Nếu có patientId, tự động chọn bệnh nhân
            if (patientId.HasValue)
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", patientId);
            }
            else
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName");
            }
            
            // Nếu có treatmentId, tự động chọn điều trị
            if (treatmentId.HasValue)
            {
                ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", treatmentId);
            }
            else
            {
                ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName");
            }
            
            // Thiết lập giá trị mặc định cho ngày và giờ
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(9, 0, 0), // 9:00 AM
                Status = "Scheduled",
                PatientId = patientId.HasValue ? patientId.Value : 0,
                TreatmentId = treatmentId
            };
            
            return View(appointment);
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,TreatmentId,AppointmentDate,AppointmentTime,Duration,Purpose,Status,Notes")] Appointment appointment)
        {
            // Kiểm tra xem bác sĩ có lịch trùng không
            var conflictingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == appointment.DoctorId && 
                           a.AppointmentDate == appointment.AppointmentDate &&
                           a.Status != "Cancelled" &&
                           ((a.AppointmentTime <= appointment.AppointmentTime && 
                             a.AppointmentTime.Add(TimeSpan.FromMinutes(a.Duration)) > appointment.AppointmentTime) ||
                            (a.AppointmentTime >= appointment.AppointmentTime && 
                             a.AppointmentTime < appointment.AppointmentTime.Add(TimeSpan.FromMinutes(appointment.Duration)))))
                .ToListAsync();

            if (conflictingAppointments.Any())
            {
                ModelState.AddModelError("AppointmentTime", "Bác sĩ đã có lịch hẹn vào thời gian này.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", appointment.PatientId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", appointment.TreatmentId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", appointment.PatientId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", appointment.TreatmentId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,TreatmentId,AppointmentDate,AppointmentTime,Duration,Purpose,Status,Notes")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            // Kiểm tra xem bác sĩ có lịch trùng không (trừ chính lịch này)
            var conflictingAppointments = await _context.Appointments
                .Where(a => a.Id != id &&
                           a.DoctorId == appointment.DoctorId && 
                           a.AppointmentDate == appointment.AppointmentDate &&
                           a.Status != "Cancelled" &&
                           ((a.AppointmentTime <= appointment.AppointmentTime && 
                             a.AppointmentTime.Add(TimeSpan.FromMinutes(a.Duration)) > appointment.AppointmentTime) ||
                            (a.AppointmentTime >= appointment.AppointmentTime && 
                             a.AppointmentTime < appointment.AppointmentTime.Add(TimeSpan.FromMinutes(appointment.Duration)))))
                .ToListAsync();

            if (conflictingAppointments.Any())
            {
                ModelState.AddModelError("AppointmentTime", "Bác sĩ đã có lịch hẹn vào thời gian này.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", appointment.PatientId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", appointment.TreatmentId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Treatment)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Calendar
        public async Task<IActionResult> Calendar(DateTime? date, int? doctorId)
        {
            DateTime selectedDate = date ?? DateTime.Today;
            
            // Lấy tất cả các cuộc hẹn trong ngày đã chọn
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate.Date == selectedDate.Date);
                
            // Lọc theo bác sĩ nếu có
            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }
            
            var appointments = await query
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
                
            ViewData["Doctors"] = new SelectList(_context.Doctors, "Id", "FullName", doctorId);
            ViewData["SelectedDate"] = selectedDate;
            ViewData["SelectedDoctorId"] = doctorId;
            
            return View(appointments);
        }

        // GET: Appointments/UpcomingReminders
        public async Task<IActionResult> UpcomingReminders()
        {
            // Lấy các cuộc hẹn sắp tới trong 7 ngày
            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);
            
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= nextWeek && a.Status == "Scheduled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
                
            return View(upcomingAppointments);
        }

        // POST: Appointments/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            
            appointment.Status = status;
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
} 