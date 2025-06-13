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
    public class ProceduresController : Controller
    {
        private readonly IProcedureService _procedureService;
        private readonly ITreatmentStageService _treatmentStageService;
        private readonly IPatientService _patientService;

        public ProceduresController(IProcedureService procedureService, ITreatmentStageService treatmentStageService, IPatientService patientService)
        {
            _procedureService = procedureService;
            _treatmentStageService = treatmentStageService;
            _patientService = patientService;
        }

        // GET: Procedures
        public async Task<IActionResult> Index(int? treatmentStageId, string searchString, string procedureType, string status)
        {
            var procedures = await _procedureService.GetAllProceduresAsync();

            // Lọc theo giai đoạn điều trị
            if (treatmentStageId.HasValue)
            {
                procedures = await _procedureService.GetProceduresByTreatmentStageAsync(treatmentStageId.Value);
            }

            // Tìm kiếm theo tên thủ thuật
            if (!string.IsNullOrEmpty(searchString))
            {
                procedures = procedures.Where(p =>
                    p.ProcedureName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            // Lọc theo loại thủ thuật
            if (!string.IsNullOrEmpty(procedureType))
            {
                procedures = await _procedureService.GetProceduresByTypeAsync(procedureType);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                procedures = await _procedureService.GetProceduresByStatusAsync(status);
            }

            var treatmentStages = await _treatmentStageService.GetAllTreatmentStagesAsync();

            ViewData["TreatmentStageId"] = new SelectList(treatmentStages, "Id", "StageName", treatmentStageId);
            ViewData["SearchString"] = searchString;
            ViewData["ProcedureTypes"] = new SelectList(
                new[] { "IVF", "IUI", "ICSI", "Egg Retrieval", "Embryo Transfer", "Hysteroscopy", "Laparoscopy" },
                procedureType);
            ViewData["Statuses"] = new SelectList(
                new[] { "Scheduled", "In Progress", "Completed", "Cancelled" },
                status);

            return View(procedures.OrderBy(p => p.ScheduledDate));
        }

        // GET: Procedures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _procedureService.GetProcedureByIdAsync(id.Value);
            if (procedure == null)
            {
                return NotFound();
            }

            return View(procedure);
        }

        // GET: Procedures/Create
        public async Task<IActionResult> Create(int? treatmentStageId)
        {
            var treatmentStages = await _treatmentStageService.GetAllTreatmentStagesAsync();

            ViewData["TreatmentStageId"] = new SelectList(treatmentStages, "Id", "StageName", treatmentStageId);
            ViewData["ProcedureTypes"] = new SelectList(
                new[] { "IVF", "IUI", "ICSI", "Egg Retrieval", "Embryo Transfer", "Hysteroscopy", "Laparoscopy" });

            var procedure = new Procedure
            {
                ScheduledDate = DateTime.Today.AddDays(1),
                Status = "Scheduled",
                TreatmentStageId = treatmentStageId ?? 0
            };

            return View(procedure);
        }

        // POST: Procedures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreatmentStageId,ProcedureName,Description,ScheduledDate,ActualDate,Status,Results,Notes")] Procedure procedure)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _procedureService.CreateProcedureAsync(procedure);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var treatmentStages = await _treatmentStageService.GetAllTreatmentStagesAsync();
            ViewData["TreatmentStageId"] = new SelectList(treatmentStages, "Id", "StageName", procedure.TreatmentStageId);
            ViewData["ProcedureTypes"] = new SelectList(
                new[] { "IVF", "IUI", "ICSI", "Egg Retrieval", "Embryo Transfer", "Hysteroscopy", "Laparoscopy" });
            return View(procedure);
        }

        // GET: Procedures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _procedureService.GetProcedureByIdAsync(id.Value);
            if (procedure == null)
            {
                return NotFound();
            }

            var treatmentStages = await _treatmentStageService.GetAllTreatmentStagesAsync();
            ViewData["TreatmentStageId"] = new SelectList(treatmentStages, "Id", "StageName", procedure.TreatmentStageId);
            ViewData["ProcedureTypes"] = new SelectList(
                new[] { "IVF", "IUI", "ICSI", "Egg Retrieval", "Embryo Transfer", "Hysteroscopy", "Laparoscopy" });
            return View(procedure);
        }

        // POST: Procedures/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreatmentStageId,ProcedureName,Description,ScheduledDate,ActualDate,Status,Results,Notes")] Procedure procedure)
        {
            if (id != procedure.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _procedureService.UpdateProcedureAsync(procedure);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var treatmentStages = await _treatmentStageService.GetAllTreatmentStagesAsync();
            ViewData["TreatmentStageId"] = new SelectList(treatmentStages, "Id", "StageName", procedure.TreatmentStageId);
            ViewData["ProcedureTypes"] = new SelectList(
                new[] { "IVF", "IUI", "ICSI", "Egg Retrieval", "Embryo Transfer", "Hysteroscopy", "Laparoscopy" });
            return View(procedure);
        }

        // GET: Procedures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _procedureService.GetProcedureByIdAsync(id.Value);
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
            try
            {
                await _procedureService.DeleteProcedureAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var procedure = await _procedureService.GetProcedureByIdAsync(id);
                return View(procedure);
            }
        }

        // Action methods cho quản lý thủ thuật
        [HttpPost]
        public async Task<IActionResult> StartProcedure(int id)
        {
            try
            {
                await _procedureService.StartProcedureAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteProcedure(int id, string results)
        {
            try
            {
                await _procedureService.CompleteProcedureAsync(id, results);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelProcedure(int id, string reason)
        {
            try
            {
                await _procedureService.CancelProcedureAsync(id, reason);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Procedures/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalProcedures = await _procedureService.GetTotalProceduresCountAsync();
            var proceduresByType = await _procedureService.GetProceduresByTypeStatisticsAsync();
            var proceduresByStatus = await _procedureService.GetProceduresByStatusStatisticsAsync();
            var successRate = await _procedureService.GetProcedureSuccessRateAsync();
            var averageDuration = await _procedureService.GetAverageProcedureDurationAsync();

            ViewBag.TotalProcedures = totalProcedures;
            ViewBag.ProceduresByType = proceduresByType;
            ViewBag.ProceduresByStatus = proceduresByStatus;
            ViewBag.SuccessRate = successRate;
            ViewBag.AverageDuration = averageDuration;

            return View();
        }
    }
} 