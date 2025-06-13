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
    public class MedicationsController : Controller
    {
        private readonly IMedicationService _medicationService;
        private readonly ITreatmentService _treatmentService;

        public MedicationsController(IMedicationService medicationService, ITreatmentService treatmentService)
        {
            _medicationService = medicationService;
            _treatmentService = treatmentService;
        }

        // GET: Medications
        public async Task<IActionResult> Index(int? treatmentId, string searchString, string medicationType)
        {
            var medications = await _medicationService.GetAllMedicationsAsync();

            // Lọc theo điều trị
            if (treatmentId.HasValue)
            {
                medications = await _medicationService.GetMedicationsByTreatmentAsync(treatmentId.Value);
            }

            // Tìm kiếm theo tên thuốc
            if (!string.IsNullOrEmpty(searchString))
            {
                medications = medications.Where(m =>
                    m.MedicationName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (m.Dosage != null && m.Dosage.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Instructions != null && m.Instructions.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            // Lọc theo loại thuốc
            if (!string.IsNullOrEmpty(medicationType))
            {
                medications = await _medicationService.GetMedicationsByTypeAsync(medicationType);
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();

            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", treatmentId);
            ViewData["SearchString"] = searchString;
            ViewData["MedicationTypes"] = new SelectList(
                new[] { "Clomiphene", "Letrozole", "Gonadotropins", "Metformin", "Progesterone", "Bromocriptine" },
                medicationType);

            return View(medications.OrderBy(m => m.StartDate));
        }

        // GET: Medications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _medicationService.GetMedicationByIdAsync(id.Value);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // GET: Medications/Create
        public async Task<IActionResult> Create(int? treatmentId)
        {
            var treatments = await _treatmentService.GetAllTreatmentsAsync();

            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", treatmentId);

            var medication = new Medication
            {
                StartDate = DateTime.Today,
                TreatmentId = treatmentId ?? 0
            };

            return View(medication);
        }

        // POST: Medications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreatmentId,MedicationName,Dosage,Frequency,StartDate,EndDate,Instructions,Notes,Status")] Medication medication)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _medicationService.CreateMedicationAsync(medication);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", medication.TreatmentId);
            return View(medication);
        }

        // GET: Medications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _medicationService.GetMedicationByIdAsync(id.Value);
            if (medication == null)
            {
                return NotFound();
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", medication.TreatmentId);
            return View(medication);
        }

        // POST: Medications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreatmentId,MedicationName,Dosage,Frequency,StartDate,EndDate,Instructions,Notes,Status")] Medication medication)
        {
            if (id != medication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _medicationService.UpdateMedicationAsync(medication);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", medication.TreatmentId);
            return View(medication);
        }

        // GET: Medications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _medicationService.GetMedicationByIdAsync(id.Value);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // POST: Medications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _medicationService.DeleteMedicationAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var medication = await _medicationService.GetMedicationByIdAsync(id);
                return View(medication);
            }
        }

        // GET: Medications/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalMedications = await _medicationService.GetTotalMedicationsCountAsync();
            var medicationsByType = await _medicationService.GetMedicationsByTypeStatisticsAsync();
            var averageDuration = await _medicationService.GetAverageMedicationDurationAsync();

            ViewBag.TotalMedications = totalMedications;
            ViewBag.MedicationsByType = medicationsByType;
            ViewBag.AverageDuration = averageDuration;

            return View();
        }

        // POST: Medications/CheckInteraction
        [HttpPost]
        public async Task<IActionResult> CheckInteraction(int patientId, string medicationName)
        {
            try
            {
                var hasInteraction = await _medicationService.CheckMedicationInteractionAsync(patientId, medicationName);
                return Json(new { hasInteraction });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
} 