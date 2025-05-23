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
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Doctors.ToListAsync());
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Appointments)
                .Include(d => d.Treatments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Specialization,PhoneNumber,Email,LicenseNumber")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Specialization,PhoneNumber,Email,LicenseNumber")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
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
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/Schedule/5
        public async Task<IActionResult> Schedule(int? id, DateTime? date)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Nếu không có ngày được chọn, sử dụng ngày hiện tại
            DateTime selectedDate = date ?? DateTime.Today;
            
            // Lấy lịch làm việc của bác sĩ trong ngày đã chọn
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == id && a.AppointmentDate.Date == selectedDate.Date)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            ViewData["Doctor"] = doctor;
            ViewData["SelectedDate"] = selectedDate;
            
            return View(appointments);
        }

        // GET: Doctors/PatientStatistics/5
        public async Task<IActionResult> PatientStatistics(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Lấy số lượng bệnh nhân đang điều trị với bác sĩ
            var activePatients = await _context.Treatments
                .Where(t => t.DoctorId == id && t.Status == "Active")
                .Select(t => t.PatientId)
                .Distinct()
                .CountAsync();

            // Lấy số lượng cuộc hẹn trong tuần này
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);
            var appointmentsThisWeek = await _context.Appointments
                .Where(a => a.DoctorId == id && a.AppointmentDate >= startOfWeek && a.AppointmentDate <= endOfWeek)
                .CountAsync();

            // Lấy số lượng điều trị thành công
            var successfulTreatments = await _context.Treatments
                .Where(t => t.DoctorId == id && t.Status == "Completed" && t.Outcome == "Successful")
                .CountAsync();

            ViewData["Doctor"] = doctor;
            ViewData["ActivePatients"] = activePatients;
            ViewData["AppointmentsThisWeek"] = appointmentsThisWeek;
            ViewData["SuccessfulTreatments"] = successfulTreatments;

            // Lấy danh sách bệnh nhân đang điều trị với bác sĩ
            var patients = await _context.Treatments
                .Where(t => t.DoctorId == id && t.Status == "Active")
                .Include(t => t.Patient)
                .Select(t => t.Patient)
                .Distinct()
                .ToListAsync();

            return View(patients);
        }

        // GET: Doctors/WorkSchedule
        public async Task<IActionResult> WorkSchedule()
        {
            // Lấy danh sách tất cả bác sĩ
            var doctors = await _context.Doctors.ToListAsync();
            
            // Lấy ngày hiện tại và 6 ngày tiếp theo
            var today = DateTime.Today;
            var dates = Enumerable.Range(0, 7).Select(i => today.AddDays(i)).ToList();
            
            // Lấy tất cả các cuộc hẹn trong 7 ngày tới
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= today.AddDays(6))
                .ToListAsync();
                
            ViewData["Doctors"] = doctors;
            ViewData["Dates"] = dates;
            ViewData["Appointments"] = appointments;
            
            return View();
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }
} 