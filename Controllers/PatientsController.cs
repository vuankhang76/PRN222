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
    public class PatientsController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // GET: Patients
        public async Task<IActionResult> Index(string searchString, string gender)
        {
            var patients = await _patientService.GetAllPatientsAsync();

            // Tìm kiếm theo tên, email, số điện thoại
            if (!string.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(p =>
                    p.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (p.Email != null && p.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    p.PhoneNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (p.Address != null && p.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            // Lọc theo giới tính
            if (!string.IsNullOrEmpty(gender))
            {
                patients = patients.Where(p => p.Gender == gender);
            }

            ViewData["SearchString"] = searchString;
            ViewData["Gender"] = gender;

            return View(patients.OrderByDescending(p => p.RegistrationDate));
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _patientService.GetPatientWithDetailsAsync(id.Value);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,DateOfBirth,Gender,PhoneNumber,Email,Address,Occupation,MedicalHistory,AllergiesInfo")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _patientService.CreatePatientAsync(patient);
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DateOfBirth,Gender,PhoneNumber,Email,Address,Occupation,MedicalHistory,AllergiesInfo,RegistrationDate")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _patientService.UpdatePatientAsync(patient);
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    if (!await PatientExistsAsync(patient.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _patientService.DeletePatientAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> PatientExistsAsync(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            return patient != null;
        }
    }
} 