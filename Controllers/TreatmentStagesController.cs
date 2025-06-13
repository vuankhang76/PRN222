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
    public class TreatmentStagesController : Controller
    {
        private readonly ITreatmentStageService _treatmentStageService;
        private readonly ITreatmentService _treatmentService;

        public TreatmentStagesController(ITreatmentStageService treatmentStageService, ITreatmentService treatmentService)
        {
            _treatmentStageService = treatmentStageService;
            _treatmentService = treatmentService;
        }

        // GET: TreatmentStages
        public async Task<IActionResult> Index(int? treatmentId, string searchString, string status)
        {
            var treatmentStages = await _treatmentStageService.GetAllTreatmentStagesAsync();

            // Lọc theo điều trị
            if (treatmentId.HasValue)
            {
                treatmentStages = await _treatmentStageService.GetTreatmentStagesByTreatmentAsync(treatmentId.Value);
            }

            // Tìm kiếm theo tên giai đoạn
            if (!string.IsNullOrEmpty(searchString))
            {
                treatmentStages = treatmentStages.Where(ts =>
                    ts.StageName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (ts.Description != null && ts.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                treatmentStages = await _treatmentStageService.GetTreatmentStagesByStatusAsync(status);
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();

            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", treatmentId);
            ViewData["SearchString"] = searchString;
            ViewData["Status"] = status;
            ViewData["Statuses"] = new SelectList(
                new[] { "Not Started", "In Progress", "Completed", "On Hold" },
                status);

            return View(treatmentStages.OrderBy(ts => ts.StageOrder));
        }

        // GET: TreatmentStages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatmentStage = await _treatmentStageService.GetTreatmentStageByIdAsync(id.Value);
            if (treatmentStage == null)
            {
                return NotFound();
            }

            return View(treatmentStage);
        }

        // GET: TreatmentStages/Create
        public async Task<IActionResult> Create(int? treatmentId)
        {
            var treatments = await _treatmentService.GetAllTreatmentsAsync();

            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", treatmentId);

            var treatmentStage = new TreatmentStage
            {
                StartDate = DateTime.Today,
                Status = "Not Started",
                TreatmentId = treatmentId ?? 0
            };

            return View(treatmentStage);
        }

        // POST: TreatmentStages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreatmentId,StageName,Description,StageOrder,StartDate,EndDate,Status,Notes")] TreatmentStage treatmentStage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _treatmentStageService.CreateTreatmentStageAsync(treatmentStage);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", treatmentStage.TreatmentId);
            return View(treatmentStage);
        }

        // GET: TreatmentStages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatmentStage = await _treatmentStageService.GetTreatmentStageByIdAsync(id.Value);
            if (treatmentStage == null)
            {
                return NotFound();
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", treatmentStage.TreatmentId);
            return View(treatmentStage);
        }

        // POST: TreatmentStages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreatmentId,StageName,Description,StageOrder,StartDate,EndDate,Status,Notes")] TreatmentStage treatmentStage)
        {
            if (id != treatmentStage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _treatmentStageService.UpdateTreatmentStageAsync(treatmentStage);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            ViewData["TreatmentId"] = new SelectList(treatments, "Id", "TreatmentName", treatmentStage.TreatmentId);
            return View(treatmentStage);
        }

        // GET: TreatmentStages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var treatmentStage = await _treatmentStageService.GetTreatmentStageByIdAsync(id.Value);
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
            try
            {
                await _treatmentStageService.DeleteTreatmentStageAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var treatmentStage = await _treatmentStageService.GetTreatmentStageByIdAsync(id);
                return View(treatmentStage);
            }
        }

        // Action methods cho quản lý giai đoạn điều trị
        [HttpPost]
        public async Task<IActionResult> StartStage(int id)
        {
            try
            {
                await _treatmentStageService.StartTreatmentStageAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteStage(int id, string results = "Hoàn thành")
        {
            try
            {
                await _treatmentStageService.CompleteTreatmentStageAsync(id, results);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MoveToNextStage(int treatmentId)
        {
            try
            {
                await _treatmentStageService.MoveToNextStageAsync(treatmentId);
                return RedirectToAction(nameof(Index), new { treatmentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { treatmentId });
            }
        }

        // GET: TreatmentStages/ByTreatment/5
        public async Task<IActionResult> ByTreatment(int? id)
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

            var treatmentStages = await _treatmentStageService.GetTreatmentStagesByTreatmentAsync(id.Value);
            ViewBag.Treatment = treatment;

            return View(treatmentStages.OrderBy(ts => ts.StageOrder));
        }

        // GET: TreatmentStages/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalStages = await _treatmentStageService.GetTotalTreatmentStagesCountAsync();
            var stagesByStatus = await _treatmentStageService.GetTreatmentStagesByStatusStatisticsAsync();
            var averageDuration = await _treatmentStageService.GetAverageStageDurationAsync();

            ViewBag.TotalStages = totalStages;
            ViewBag.StagesByStatus = stagesByStatus;
            ViewBag.AverageDuration = averageDuration;

            return View();
        }
    }
} 