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
    public class PartnersController : Controller
    {
        private readonly IPartnerService _partnerService;
        private readonly IPatientService _patientService;

        public PartnersController(IPartnerService partnerService, IPatientService patientService)
        {
            _partnerService = partnerService;
            _patientService = patientService;
        }

        // GET: Partners
        public async Task<IActionResult> Index(string searchString, string gender, int? ageFrom, int? ageTo)
        {
            var partners = await _partnerService.GetAllPartnersAsync();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchString))
            {
                partners = partners.Where(p =>
                    p.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (p.Email != null && p.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            // Lọc theo giới tính
            if (!string.IsNullOrEmpty(gender))
            {
                partners = partners.Where(p => p.Gender == gender);
            }

            // Lọc theo độ tuổi
            if (ageFrom.HasValue && ageTo.HasValue)
            {
                partners = await _partnerService.GetPartnersByAgeRangeAsync(ageFrom.Value, ageTo.Value);
            }

            ViewData["SearchString"] = searchString;
            ViewData["Gender"] = gender;
            ViewData["AgeFrom"] = ageFrom;
            ViewData["AgeTo"] = ageTo;
            ViewData["Genders"] = new SelectList(new[] { "Nam", "Nữ" }, gender);

            return View(partners.OrderBy(p => p.FullName));
        }

        // GET: Partners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partner = await _partnerService.GetPartnerByIdAsync(id.Value);
            if (partner == null)
            {
                return NotFound();
            }

            return View(partner);
        }

        // GET: Partners/Create
        public async Task<IActionResult> Create(int? patientId)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", patientId);

            var partner = new Partner
            {
                PatientId = patientId ?? 0
            };

            return View(partner);
        }

        // POST: Partners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,FullName,DateOfBirth,Gender,PhoneNumber,Email,Address,Occupation,MedicalHistory")] Partner partner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _partnerService.CreatePartnerAsync(partner);
                    return RedirectToAction("Details", "Patients", new { id = partner.PatientId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", partner.PatientId);
            return View(partner);
        }

        // GET: Partners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partner = await _partnerService.GetPartnerByIdAsync(id.Value);
            if (partner == null)
            {
                return NotFound();
            }

            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", partner.PatientId);
            return View(partner);
        }

        // POST: Partners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,FullName,DateOfBirth,Gender,PhoneNumber,Email,Address,Occupation,MedicalHistory")] Partner partner)
        {
            if (id != partner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _partnerService.UpdatePartnerAsync(partner);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", partner.PatientId);
            return View(partner);
        }

        // GET: Partners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partner = await _partnerService.GetPartnerByIdAsync(id.Value);
            if (partner == null)
            {
                return NotFound();
            }

            return View(partner);
        }

        // POST: Partners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _partnerService.DeletePartnerAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var partner = await _partnerService.GetPartnerByIdAsync(id);
                return View(partner);
            }
        }

        // GET: Partners/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalPartners = await _partnerService.GetTotalPartnersCountAsync();
            var partnersByGender = await _partnerService.GetPartnersByGenderStatisticsAsync();
            var averageAge = await _partnerService.GetAveragePartnerAgeAsync();

            ViewBag.TotalPartners = totalPartners;
            ViewBag.PartnersByGender = partnersByGender;
            ViewBag.AverageAge = averageAge;

            return View();
        }

        // GET: Partners/ByPatient/5
        public async Task<IActionResult> ByPatient(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _patientService.GetPatientByIdAsync(id.Value);
            if (patient == null)
            {
                return NotFound();
            }

            var partner = await _partnerService.GetPartnerByPatientIdAsync(id.Value);
            ViewBag.Patient = patient;

            if (partner == null)
            {
                return View("NoPartner", patient);
            }

            return View(partner);
        }

        // GET: Partners/PatientPartner/5
        public async Task<IActionResult> PatientPartner(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            var partner = await _partnerService.GetPartnerByPatientIdAsync(id);
            ViewBag.Patient = patient;
            ViewBag.PatientId = id;

            if (partner == null)
            {
                // Redirect to create partner page if no partner exists
                return RedirectToAction("Create", new { patientId = id });
            }

            return View(partner);
        }
    }
} 