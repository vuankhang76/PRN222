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
    public class ProceduresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProceduresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Procedures
        public async Task<IActionResult> Index()
        {
            var procedures = await _context.Procedures
                .Include(p => p.TreatmentStage)
                .ThenInclude(ts => ts.Treatment)
                .ThenInclude(t => t.Patient)
                .OrderBy(p => p.ScheduledDate)
                .ToListAsync();
            return View(procedures);
        }

        // GET: Procedures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _context.Procedures
                .Include(p => p.TreatmentStage)
                .ThenInclude(ts => ts.Treatment)
                .ThenInclude(t => t.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (procedure == null)
            {
                return NotFound();
            }

            return View(procedure);
        }

        // GET: Procedures/Create
        public IActionResult Create(int? treatmentStageId)
        {
            if (treatmentStageId.HasValue)
            {
                ViewData["TreatmentStageId"] = new SelectList(_context.TreatmentStages.Where(ts => ts.Id == treatmentStageId), "Id", "StageName");
                ViewData["FixedTreatmentStageId"] = treatmentStageId;
                
                var treatmentStage = _context.TreatmentStages
                    .Include(ts => ts.Treatment)
                    .ThenInclude(t => t.Patient)
                    .FirstOrDefault(ts => ts.Id == treatmentStageId);
                
                if (treatmentStage != null)
                {
                    ViewData["TreatmentId"] = treatmentStage.TreatmentId;
                    ViewData["TreatmentName"] = treatmentStage.Treatment.TreatmentName;
                    ViewData["PatientId"] = treatmentStage.Treatment.PatientId;
                    ViewData["PatientName"] = treatmentStage.Treatment.Patient.FullName;
                }
            }
            else
            {
                ViewData["TreatmentStageId"] = new SelectList(_context.TreatmentStages, "Id", "StageName");
            }
            return View();
        }

        // POST: Procedures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreatmentStageId,ProcedureName,ScheduledDate,ActualDate,Status,Description,Results,Notes,Cost")] Procedure procedure)
        {
            if (ModelState.IsValid)
            {
                _context.Add(procedure);
                await _context.SaveChangesAsync();
                
                var treatmentStage = await _context.TreatmentStages
                    .FirstOrDefaultAsync(ts => ts.Id == procedure.TreatmentStageId);
                
                return RedirectToAction("Details", "TreatmentStages", new { id = procedure.TreatmentStageId });
            }
            ViewData["TreatmentStageId"] = new SelectList(_context.TreatmentStages, "Id", "StageName", procedure.TreatmentStageId);
            return View(procedure);
        }

        // GET: Procedures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _context.Procedures.FindAsync(id);
            if (procedure == null)
            {
                return NotFound();
            }
            ViewData["TreatmentStageId"] = new SelectList(_context.TreatmentStages, "Id", "StageName", procedure.TreatmentStageId);
            return View(procedure);
        }

        // POST: Procedures/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreatmentStageId,ProcedureName,ScheduledDate,ActualDate,Status,Description,Results,Notes,Cost")] Procedure procedure)
        {
            if (id != procedure.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(procedure);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcedureExists(procedure.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "TreatmentStages", new { id = procedure.TreatmentStageId });
            }
            ViewData["TreatmentStageId"] = new SelectList(_context.TreatmentStages, "Id", "StageName", procedure.TreatmentStageId);
            return View(procedure);
        }

        // GET: Procedures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _context.Procedures
                .Include(p => p.TreatmentStage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (procedure == null)
            {
                return NotFound();
            }

            return View(procedure);
        }

        // POST: Procedures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var procedure = await _context.Procedures.FindAsync(id);
            if (procedure == null)
            {
                return NotFound();
            }
            
            var treatmentStageId = procedure.TreatmentStageId;
            _context.Procedures.Remove(procedure);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", "TreatmentStages", new { id = treatmentStageId });
        }

        // GET: Procedures/StageProcedures/5
        public async Task<IActionResult> StageProcedures(int? id)
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

            ViewData["TreatmentStageId"] = treatmentStage.Id;
            ViewData["StageName"] = treatmentStage.StageName;
            ViewData["TreatmentId"] = treatmentStage.TreatmentId;
            ViewData["TreatmentName"] = treatmentStage.Treatment.TreatmentName;
            ViewData["PatientId"] = treatmentStage.Treatment.PatientId;
            ViewData["PatientName"] = treatmentStage.Treatment.Patient.FullName;

            return View(treatmentStage.Procedures.OrderBy(p => p.ScheduledDate).ToList());
        }

        private bool ProcedureExists(int id)
        {
            return _context.Procedures.Any(e => e.Id == id);
        }
    }
} 