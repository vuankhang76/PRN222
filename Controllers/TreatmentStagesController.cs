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
    public class TreatmentStagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TreatmentStagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TreatmentStages
        public async Task<IActionResult> Index()
        {
            var treatmentStages = await _context.TreatmentStages
                .Include(ts => ts.Treatment)
                .ThenInclude(t => t.Patient)
                .OrderBy(ts => ts.Treatment.PatientId)
                .ThenBy(ts => ts.Treatment.Id)
                .ThenBy(ts => ts.StageOrder)
                .ToListAsync();
            return View(treatmentStages);
        }

        // GET: TreatmentStages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatmentStage = await _context.TreatmentStages
                .Include(ts => ts.Treatment)
                .ThenInclude(t => t.Patient)
                .Include(ts => ts.Procedures)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (treatmentStage == null)
            {
                return NotFound();
            }

            return View(treatmentStage);
        }

        // GET: TreatmentStages/Create
        public IActionResult Create(int? treatmentId)
        {
            if (treatmentId.HasValue)
            {
                ViewData["TreatmentId"] = new SelectList(_context.Treatments.Where(t => t.Id == treatmentId), "Id", "TreatmentName");
                ViewData["FixedTreatmentId"] = treatmentId;
            }
            else
            {
                ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName");
            }
            return View();
        }

        // POST: TreatmentStages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreatmentId,StageName,StageOrder,StartDate,EndDate,Status,Description,Notes,Results")] TreatmentStage treatmentStage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(treatmentStage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Treatments", new { id = treatmentStage.TreatmentId });
            }
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", treatmentStage.TreatmentId);
            return View(treatmentStage);
        }

        // GET: TreatmentStages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatmentStage = await _context.TreatmentStages.FindAsync(id);
            if (treatmentStage == null)
            {
                return NotFound();
            }
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", treatmentStage.TreatmentId);
            return View(treatmentStage);
        }

        // POST: TreatmentStages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreatmentId,StageName,StageOrder,StartDate,EndDate,Status,Description,Notes,Results")] TreatmentStage treatmentStage)
        {
            if (id != treatmentStage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(treatmentStage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreatmentStageExists(treatmentStage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Treatments", new { id = treatmentStage.TreatmentId });
            }
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "Id", "TreatmentName", treatmentStage.TreatmentId);
            return View(treatmentStage);
        }

        // GET: TreatmentStages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatmentStage = await _context.TreatmentStages
                .Include(ts => ts.Treatment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (treatmentStage == null)
            {
                return NotFound();
            }

            return View(treatmentStage);
        }

        // POST: TreatmentStages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var treatmentStage = await _context.TreatmentStages.FindAsync(id);
            if (treatmentStage == null)
            {
                return NotFound();
            }
            
            var treatmentId = treatmentStage.TreatmentId;
            _context.TreatmentStages.Remove(treatmentStage);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", "Treatments", new { id = treatmentId });
        }

        // GET: TreatmentStages/TreatmentStages/5
        public async Task<IActionResult> TreatmentStages(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatment = await _context.Treatments
                .Include(t => t.Patient)
                .Include(t => t.Doctor)
                .Include(t => t.TreatmentStages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (treatment == null)
            {
                return NotFound();
            }

            ViewData["TreatmentName"] = treatment.TreatmentName;
            ViewData["TreatmentId"] = treatment.Id;
            ViewData["PatientName"] = treatment.Patient.FullName;
            ViewData["PatientId"] = treatment.PatientId;

            return View(treatment.TreatmentStages.OrderBy(ts => ts.StageOrder).ToList());
        }

        private bool TreatmentStageExists(int id)
        {
            return _context.TreatmentStages.Any(e => e.Id == id);
        }
    }
} 