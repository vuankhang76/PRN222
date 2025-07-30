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
    public class TreatmentsController : Controller
    {
        private readonly ITreatmentService _treatmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public TreatmentsController(ITreatmentService treatmentService, IPatientService patientService, IDoctorService doctorService)
        {
            _treatmentService = treatmentService;
            _patientService = patientService;
            _doctorService = doctorService;
        }

        // GET: Treatments/GetByMedicalRecord/5
        public async Task<IActionResult> GetByMedicalRecord(int id)
        {
            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            treatments = treatments.Where(t => t.MedicalRecordId == id);

            ViewBag.MedicalRecordId = id;
            return View("Index", treatments);
        }

        // GET: Treatments
        public async Task<IActionResult> Index(string searchString, string status, int? patientId, int? doctorId)
        {
            var treatments = await _treatmentService.GetAllTreatmentsAsync();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                treatments = await _treatmentService.GetTreatmentsByStatusAsync(status);
            }

            // Lọc theo bệnh nhân
            if (patientId.HasValue)
            {
                treatments = await _treatmentService.GetTreatmentsByPatientAsync(patientId.Value);
            }

            // Lọc theo bác sĩ
            if (doctorId.HasValue)
            {
                treatments = await _treatmentService.GetTreatmentsByDoctorAsync(doctorId.Value);
            }

            // Lọc theo tên bệnh nhân
            if (!string.IsNullOrEmpty(searchString))
            {
                var allTreatments = await _treatmentService.GetAllTreatmentsAsync();
                treatments = allTreatments.Where(t =>
                    t.Patient != null && t.Patient.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    t.TreatmentName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    t.TreatmentType.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();

            ViewData["Patients"] = new SelectList(patients, "Id", "FullName", patientId);
            ViewData["Doctors"] = new SelectList(doctors, "Id", "FullName", doctorId);
            ViewData["Statuses"] = new SelectList(new[] { "Đang điều trị", "Hoàn thành", "Tạm dừng", "Đã hủy" }, status);
            ViewData["SearchString"] = searchString;

            return View(treatments);
        }

        // GET: Treatments/PatientTreatments/5
        public async Task<IActionResult> PatientTreatments(int id)
        {
            // Get the patient first to ensure they exist
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Get all treatments for this patient
            var treatments = await _treatmentService.GetTreatmentsByPatientAsync(id);

            ViewData["PatientName"] = patient.FullName;
            ViewData["PatientId"] = id;

            return View(treatments);
        }

        // GET: Treatments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _treatmentService.GetTreatmentWithDetailsAsync(id.Value);
            if (treatment == null)
            {
                return NotFound();
            }

            return View(treatment);
        }

        // GET: Treatments/Create
        public async Task<IActionResult> Create(int? patientId, int? appointmentId)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();

            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName");
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", patientId);
            ViewBag.AppointmentId = appointmentId;

            var treatment = new Treatment
            {
                StartDate = DateTime.Today,
                Status = Treatment.TreatmentStatus.InProgress,
                PatientId = patientId.HasValue ? patientId.Value : 0,
                AppointmentId = appointmentId
            };

            return View(treatment);
        }

        // POST: Treatments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,TreatmentType,TreatmentName,Description,StartDate,Status,Notes,AppointmentId")] Treatment treatment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _treatmentService.CreateTreatmentAsync(treatment);
                    TempData["Success"] = "Điều trị đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi tạo điều trị: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }

            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", treatment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", treatment.PatientId);
            return View(treatment);
        }

        // GET: Treatments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _treatmentService.GetTreatmentByIdAsync(id.Value);
            if (treatment == null)
            {
                return NotFound();
            }

            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", treatment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", treatment.PatientId);
            return View(treatment);
        }

        // POST: Treatments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,TreatmentType,TreatmentName,Description,StartDate,EndDate,Status,Outcome,Notes")] Treatment treatment)
        {
            if (id != treatment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _treatmentService.UpdateTreatmentAsync(treatment);
                    TempData["Success"] = "Điều trị đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi cập nhật điều trị: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }

            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", treatment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", treatment.PatientId);
            return View(treatment);
        }

        // GET: Treatments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _treatmentService.GetTreatmentWithDetailsAsync(id.Value);
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
            try
            {
                await _treatmentService.DeleteTreatmentAsync(id);
                TempData["Success"] = "Điều trị đã được xóa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa điều trị: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Action methods cho quản lý trạng thái điều trị
        [HttpPost]
        public async Task<IActionResult> StartTreatment(int id)
        {
            try
            {
                await _treatmentService.StartTreatmentAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTreatment(int id, string results)
        {
            try
            {
                await _treatmentService.CompleteTreatmentAsync(id, results);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PauseTreatment(int id, string reason)
        {
            try
            {
                await _treatmentService.PauseTreatmentAsync(id, reason);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResumeTreatment(int id)
        {
            try
            {
                await _treatmentService.ResumeTreatmentAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Treatments/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalTreatments = await _treatmentService.GetTotalTreatmentsCountAsync();
            var treatmentsByStatus = await _treatmentService.GetTreatmentsByStatusStatisticsAsync();
            var treatmentsByType = await _treatmentService.GetTreatmentsByTypeStatisticsAsync();
            var averageDuration = await _treatmentService.GetAverageTreatmentDurationAsync();
            var successRate = await _treatmentService.GetTreatmentSuccessRateAsync();

            ViewBag.TotalTreatments = totalTreatments;
            ViewBag.TreatmentsByStatus = treatmentsByStatus;
            ViewBag.TreatmentsByType = treatmentsByType;
            ViewBag.AverageDuration = averageDuration;
            ViewBag.SuccessRate = successRate;

            return View();
        }

        // Action cho tính năng chart thống kê
        public async Task<IActionResult> GetTreatmentStatistics()
        {
            var statistics = new
            {
                statusStats = await _treatmentService.GetTreatmentsByStatusStatisticsAsync(),
                typeStats = await _treatmentService.GetTreatmentsByTypeStatisticsAsync(),
                totalCount = await _treatmentService.GetTotalTreatmentsCountAsync(),
                averageDuration = await _treatmentService.GetAverageTreatmentDurationAsync(),
                successRate = await _treatmentService.GetTreatmentSuccessRateAsync()
            };

            return Json(statistics);
        }
    }
} 