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
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET: Doctors
        public async Task<IActionResult> Index(string searchString, string specialization)
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                doctors = doctors.Where(d =>
                    d.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    d.Specialization.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    d.LicenseNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(specialization))
            {
                doctors = await _doctorService.GetDoctorsBySpecializationAsync(specialization);
            }

            ViewData["SearchString"] = searchString;
            ViewData["Specializations"] = new SelectList(
                new[] { "Reproductive Endocrinology", "Obstetrics and Gynecology", "Urology", "Andrology" },
                specialization);

            return View(doctors);
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _doctorService.GetDoctorByIdAsync(id.Value);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,DateOfBirth,Gender,Email,PhoneNumber,Address,Specialization,LicenseNumber,YearsOfExperience")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _doctorService.CreateDoctorAsync(doctor);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _doctorService.GetDoctorByIdAsync(id.Value);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DateOfBirth,Gender,Email,PhoneNumber,Address,Specialization,LicenseNumber,YearsOfExperience")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _doctorService.UpdateDoctorAsync(doctor);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _doctorService.GetDoctorByIdAsync(id.Value);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _doctorService.DeleteDoctorAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                return View(doctor);
            }
        }

        // GET: Doctors/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalDoctors = await _doctorService.GetTotalDoctorsCountAsync();
            var doctorsBySpecialization = await _doctorService.GetDoctorsBySpecializationStatisticsAsync();
            var availableDoctors = (await _doctorService.GetAvailableDoctorsAsync()).Count();

            ViewBag.TotalDoctors = totalDoctors;
            ViewBag.DoctorsBySpecialization = doctorsBySpecialization;
            ViewBag.AvailableDoctors = availableDoctors;

            return View();
        }

        // Action cho tính năng chart thống kê
        public async Task<IActionResult> GetDoctorStatistics()
        {
            var statistics = new
            {
                specializationStats = await _doctorService.GetDoctorsBySpecializationStatisticsAsync(),
                totalCount = await _doctorService.GetTotalDoctorsCountAsync(),
                availableCount = (await _doctorService.GetAvailableDoctorsAsync()).Count()
            };

            return Json(statistics);
        }
    }
} 