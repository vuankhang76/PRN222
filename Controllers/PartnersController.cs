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
    public class PartnersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PartnersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Partners
        public async Task<IActionResult> Index()
        {
            var partners = await _context.Partners
                .Include(p => p.Patient)
                .ToListAsync();
            return View(partners);
        }

        // GET: Partners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partner = await _context.Partners
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (partner == null)
            {
                return NotFound();
            }

            return View(partner);
        }

        // GET: Partners/Create
        public IActionResult Create(int? patientId)
        {
            // Check if patient already has a partner
            if (patientId.HasValue)
            {
                var existingPartner = _context.Partners.FirstOrDefault(p => p.PatientId == patientId);
                if (existingPartner != null)
                {
                    return RedirectToAction("Edit", new { id = existingPartner.Id });
                }
                
                ViewData["PatientId"] = new SelectList(_context.Patients.Where(p => p.Id == patientId), "Id", "FullName");
                ViewData["FixedPatientId"] = patientId;
                
                var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);
                if (patient != null)
                {
                    ViewData["PatientName"] = patient.FullName;
                }
            }
            else
            {
                // Get patients without partners
                var patientsWithoutPartners = _context.Patients
                    .Where(p => !_context.Partners.Select(partner => partner.PatientId).Contains(p.Id))
                    .ToList();
                ViewData["PatientId"] = new SelectList(patientsWithoutPartners, "Id", "FullName");
            }
            
            return View();
        }

        // POST: Partners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,DateOfBirth,Gender,PhoneNumber,Email,Occupation,MedicalHistory,PatientId")] Partner partner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(partner);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Patients", new { id = partner.PatientId });
            }
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", partner.PatientId);
            return View(partner);
        }

        // GET: Partners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
            {
                return NotFound();
            }
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", partner.PatientId);
            ViewData["FixedPatientId"] = partner.PatientId;
            return View(partner);
        }

        // POST: Partners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DateOfBirth,Gender,PhoneNumber,Email,Occupation,MedicalHistory,PatientId")] Partner partner)
        {
            if (id != partner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(partner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartnerExists(partner.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Patients", new { id = partner.PatientId });
            }
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", partner.PatientId);
            return View(partner);
        }

        // GET: Partners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partner = await _context.Partners
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
            {
                return NotFound();
            }
            
            var patientId = partner.PatientId;
            _context.Partners.Remove(partner);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", "Patients", new { id = patientId });
        }

        // GET: Partners/PatientPartner/5
        public async Task<IActionResult> PatientPartner(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partner = await _context.Partners
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (partner == null)
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                {
                    return NotFound();
                }
                
                ViewData["PatientId"] = patient.Id;
                ViewData["PatientName"] = patient.FullName;
                ViewData["HasPartner"] = false;
                
                return View();
            }

            ViewData["PatientId"] = partner.PatientId;
            ViewData["PatientName"] = partner.Patient.FullName;
            ViewData["HasPartner"] = true;
            ViewData["PartnerId"] = partner.Id;

            return View(partner);
        }

        private bool PartnerExists(int id)
        {
            return _context.Partners.Any(e => e.Id == id);
        }
    }
} 