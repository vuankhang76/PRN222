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
    public class MedicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Medications
        public async Task<IActionResult> Index()
        {
            var medications = await _context.Medications
                .Include(m => m.Treatment)
                .ThenInclude(t => t.Patient)
                .OrderBy(m => m.Treatment.PatientId)
                .ThenBy(m => m.StartDate)
                .ToListAsync();
            return View(medications);
        }

        // GET: Medications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications
                .Include(m => m.Treatment)
                .ThenInclude(t => t.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // GET: Medications/Create
        public IActionResult Create(int? treatmentId)
        {
            if (treatmentId.HasValue)
            {
                ViewData["TreatmentId"] = new SelectList(_context.Treatments.Where(t => t.Id == treatmentId), "Id", "TreatmentName");
                ViewData["FixedTreatmentId"] = treatmentId;
                
                var treatment = _context.Treatments
                    .Include(t => t.Patient)
                    .FirstOrDefault(t => t.Id == treatmentId);
                
                if (treatment != null)
                {
                    ViewData["PatientId"] = treatment.PatientId;
                    ViewData["PatientName"] = treatment.Patient.FullName;
                }
            }
            else
            {
                ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName");
            }
            return View();
        }

        // POST: Medications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreatmentId,MedicationName,Dosage,Instructions,StartDate,EndDate,Frequency,Notes,Status")] Medication medication)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medication);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Treatments", new { id = medication.TreatmentId });
            }
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", medication.TreatmentId);
            return View(medication);
        }

        // GET: Medications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", medication.TreatmentId);
            return View(medication);
        }

        // POST: Medications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreatmentId,MedicationName,Dosage,Instructions,StartDate,EndDate,Frequency,Notes,Status")] Medication medication)
        {
            if (id != medication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Treatments", new { id = medication.TreatmentId });
            }
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", medication.TreatmentId);
            return View(medication);
        }

        // GET: Medications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications
                .Include(m => m.Treatment)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }
            
            var treatmentId = medication.TreatmentId;
            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", "Treatments", new { id = treatmentId });
        }

        // GET: Medications/TreatmentMedications/5
        public async Task<IActionResult> TreatmentMedications(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments
                .Include(t => t.Patient)
                .Include(t => t.Medications)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (treatment == null)
            {
                return NotFound();
            }

            ViewData["TreatmentId"] = treatment.Id;
            ViewData["TreatmentName"] = treatment.TreatmentName;
            ViewData["PatientId"] = treatment.PatientId;
            ViewData["PatientName"] = treatment.Patient.FullName;

            return View(treatment.Medications.OrderBy(m => m.StartDate).ToList());
        }

        private bool MedicationExists(int id)
        {
            return _context.Medications.Any(e => e.Id == id);
        }
    }
} 