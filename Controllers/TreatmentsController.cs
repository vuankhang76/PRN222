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
    public class TreatmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TreatmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Treatments
        public async Task<IActionResult> Index(string searchString, string status, int? doctorId, int? patientId)
        {
            var treatments = _context.Treatments
                .Include(t => t.Doctor)
                .Include(t => t.Patient)
                .AsQueryable();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                treatments = treatments.Where(t => 
                    t.TreatmentName.Contains(searchString) || 
                    t.Patient.FullName.Contains(searchString) ||
                    t.Doctor.FullName.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                treatments = treatments.Where(t => t.Status == status);
            }

            // Lọc theo bác sĩ
            if (doctorId.HasValue)
            {
                treatments = treatments.Where(t => t.DoctorId == doctorId.Value);
            }

            // Lọc theo bệnh nhân
            if (patientId.HasValue)
            {
                treatments = treatments.Where(t => t.PatientId == patientId.Value);
            }

            // Chuẩn bị dữ liệu cho dropdown
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName");
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName");
            ViewData["StatusList"] = new List<string> { "Active", "Completed", "Cancelled", "On Hold" };

            // Lưu các tham số lọc để hiển thị lại khi refresh trang
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentDoctorId"] = doctorId;
            ViewData["CurrentPatientId"] = patientId;

            return View(await treatments.OrderByDescending(t => t.StartDate).ToListAsync());
        }

        // GET: Treatments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments
                .Include(t => t.Doctor)
                .Include(t => t.Patient)
                .Include(t => t.TreatmentStages)
                .Include(t => t.Medications)
                .Include(t => t.Appointments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (treatment == null)
            {
                return NotFound();
            }

            return View(treatment);
        }

        // GET: Treatments/Create
        public IActionResult Create(int? patientId)
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName");
            
            // Nếu có patientId, tự động chọn bệnh nhân
            if (patientId.HasValue)
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", patientId);
            }
            else
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName");
            }
            
            // Thiết lập giá trị mặc định
            var treatment = new Treatment
            {
                StartDate = DateTime.Today,
                Status = "Active",
                PatientId = patientId.HasValue ? patientId.Value : 0
            };
            
            return View(treatment);
        }

        // POST: Treatments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,StartDate,EndDate,TreatmentType,TreatmentName,Description,Status,Notes")] Treatment treatment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(treatment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", treatment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", treatment.PatientId);
            return View(treatment);
        }

        // GET: Treatments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", treatment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", treatment.PatientId);
            return View(treatment);
        }

        // POST: Treatments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,StartDate,EndDate,TreatmentType,TreatmentName,Description,Status,Notes,Outcome")] Treatment treatment)
        {
            if (id != treatment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu trạng thái là "Completed", đảm bảo có ngày kết thúc
                    if (treatment.Status == "Completed" && !treatment.EndDate.HasValue)
                    {
                        treatment.EndDate = DateTime.Today;
                    }
                    
                    _context.Update(treatment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreatmentExists(treatment.Id))
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
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", treatment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", treatment.PatientId);
            return View(treatment);
        }

        // GET: Treatments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments
                .Include(t => t.Doctor)
                .Include(t => t.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (treatment == null)
            {
                return NotFound();
            }

            return View(treatment);
        }

        // POST: Treatments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment != null)
            {
                _context.Treatments.Remove(treatment);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Treatments/Progress/5
        public async Task<IActionResult> Progress(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments
                .Include(t => t.Doctor)
                .Include(t => t.Patient)
                .Include(t => t.TreatmentStages.OrderBy(ts => ts.StartDate))
                .Include(t => t.Medications.OrderBy(m => m.StartDate))
                .Include(t => t.Appointments.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (treatment == null)
            {
                return NotFound();
            }

            return View(treatment);
        }

        // GET: Treatments/UpdateOutcome/5
        public async Task<IActionResult> UpdateOutcome(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments
                .Include(t => t.Doctor)
                .Include(t => t.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (treatment == null)
            {
                return NotFound();
            }

            ViewData["OutcomeOptions"] = new List<string> { "Successful", "Unsuccessful", "Ongoing" };
            return View(treatment);
        }

        // POST: Treatments/UpdateOutcome/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOutcome(int id, [Bind("Id,Outcome,Notes")] Treatment treatmentUpdate)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                treatment.Outcome = treatmentUpdate.Outcome;
                
                // Nếu kết quả là "Successful" hoặc "Unsuccessful", cập nhật trạng thái và ngày kết thúc
                if (treatmentUpdate.Outcome == "Successful" || treatmentUpdate.Outcome == "Unsuccessful")
                {
                    treatment.Status = "Completed";
                    if (!treatment.EndDate.HasValue)
                    {
                        treatment.EndDate = DateTime.Today;
                    }
                }
                
                // Cập nhật ghi chú nếu có
                if (!string.IsNullOrEmpty(treatmentUpdate.Notes))
                {
                    treatment.Notes = treatmentUpdate.Notes;
                }
                
                _context.Update(treatment);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Details), new { id = treatment.Id });
            }
            
            ViewData["OutcomeOptions"] = new List<string> { "Successful", "Unsuccessful", "Ongoing" };
            return View(treatment);
        }

        // GET: Treatments/Statistics
        public async Task<IActionResult> Statistics(int? year)
        {
            // Nếu không có năm được chọn, sử dụng năm hiện tại
            int selectedYear = year ?? DateTime.Today.Year;
            
            // Lấy tất cả các điều trị đã hoàn thành trong năm đã chọn
            var completedTreatments = await _context.Treatments
                .Where(t => t.Status == "Completed" && 
                           t.EndDate.HasValue && 
                           t.EndDate.Value.Year == selectedYear)
                .ToListAsync();
            
            // Thống kê theo loại điều trị
            var treatmentTypeStats = completedTreatments
                .GroupBy(t => t.TreatmentType)
                .Select(g => new { 
                    TreatmentType = g.Key, 
                    Count = g.Count(),
                    SuccessCount = g.Count(t => t.Outcome == "Successful"),
                    SuccessRate = g.Count() > 0 ? (double)g.Count(t => t.Outcome == "Successful") / g.Count() * 100 : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();
            
            // Thống kê theo tháng
            var monthlyStats = Enumerable.Range(1, 12)
                .Select(month => new {
                    Month = month,
                    MonthName = new DateTime(selectedYear, month, 1).ToString("MMMM"),
                    Count = completedTreatments.Count(t => t.EndDate.HasValue && t.EndDate.Value.Month == month),
                    SuccessCount = completedTreatments.Count(t => t.EndDate.HasValue && t.EndDate.Value.Month == month && t.Outcome == "Successful")
                })
                .ToList();
            
            ViewData["TreatmentTypeStats"] = treatmentTypeStats;
            ViewData["MonthlyStats"] = monthlyStats;
            ViewData["SelectedYear"] = selectedYear;
            ViewData["TotalTreatments"] = completedTreatments.Count;
            ViewData["SuccessfulTreatments"] = completedTreatments.Count(t => t.Outcome == "Successful");
            ViewData["OverallSuccessRate"] = completedTreatments.Count > 0 ? 
                (double)completedTreatments.Count(t => t.Outcome == "Successful") / completedTreatments.Count * 100 : 0;
            
            // Lấy danh sách các năm có dữ liệu để hiển thị dropdown
            var years = await _context.Treatments
                .Where(t => t.EndDate.HasValue)
                .Select(t => t.EndDate.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            
            ViewData["Years"] = years;
            
            return View();
        }

        private bool TreatmentExists(int id)
        {
            return _context.Treatments.Any(e => e.Id == id);
        }
    }
} 