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
    public class MedicalRecordsController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService, IPatientService patientService, IDoctorService doctorService)
        {
            _medicalRecordService = medicalRecordService;
            _patientService = patientService;
            _doctorService = doctorService;
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index(int? patientId, string searchString, DateTime? fromDate, DateTime? toDate, string recordType)
        {
            var medicalRecords = await _medicalRecordService.GetAllMedicalRecordsWithDetailsAsync();

            // Lọc theo bệnh nhân
            if (patientId.HasValue)
            {
                medicalRecords = await _medicalRecordService.GetMedicalRecordsByPatientAsync(patientId.Value);
            }

            // Tìm kiếm theo chẩn đoán
            if (!string.IsNullOrEmpty(searchString))
            {
                medicalRecords = await _medicalRecordService.SearchMedicalRecordsByDiagnosisAsync(searchString);
            }

            // Lọc theo loại hồ sơ
            if (!string.IsNullOrEmpty(recordType))
            {
                medicalRecords = medicalRecords.Where(mr => mr.RecordType == recordType);
            }

            // Lọc theo ngày
            if (fromDate.HasValue && toDate.HasValue)
            {
                medicalRecords = medicalRecords.Where(mr => 
                    mr.RecordDate >= fromDate.Value && mr.RecordDate <= toDate.Value);
            }

            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", patientId);
            
            // Thêm danh sách các loại hồ sơ
            var recordTypes = new List<string> 
            { 
                "Khám ban đầu", 
                "Khám theo dõi", 
                "Xét nghiệm", 
                "Siêu âm", 
                "Tư vấn", 
                "Điều trị",
                "Kết quả điều trị"
            };
            ViewData["RecordTypes"] = recordTypes;
            
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentRecordType"] = recordType;

            return View(medicalRecords.OrderByDescending(mr => mr.RecordDate));
        }

        // GET: MedicalRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _medicalRecordService.GetMedicalRecordWithDetailsAsync(id.Value);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // GET: MedicalRecords/Create
        public async Task<IActionResult> Create(int? patientId)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", patientId);

            var recordTypes = new List<string> 
            { 
                "Khám ban đầu", 
                "Khám theo dõi", 
                "Xét nghiệm", 
                "Siêu âm", 
                "Tư vấn", 
                "Điều trị",
                "Kết quả điều trị"
            };
            ViewData["RecordTypes"] = recordTypes;

            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName");

            var medicalRecord = new MedicalRecord
            {
                RecordDate = DateTime.Today,
                PatientId = patientId ?? 0
            };

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,RecordDate,RecordType,RecordTitle,Description,Results,Diagnosis,Notes,DoctorId,AttachmentPath")] MedicalRecord medicalRecord)
        {
            if (medicalRecord == null)
            {
                medicalRecord = new MedicalRecord
                {
                    RecordDate = DateTime.Today
                };
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _medicalRecordService.CreateMedicalRecordAsync(medicalRecord);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", medicalRecord.PatientId);
            var recordTypes = new List<string> 
            { 
                "Khám ban đầu", 
                "Khám theo dõi", 
                "Xét nghiệm", 
                "Siêu âm", 
                "Tư vấn", 
                "Điều trị",
                "Kết quả điều trị"
            };
            ViewData["RecordTypes"] = recordTypes;
            
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName");
            
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _medicalRecordService.GetMedicalRecordByIdAsync(id.Value);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", medicalRecord.PatientId);
            
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", medicalRecord.DoctorId);
            
            var recordTypes = new List<string> 
            { 
                "Khám ban đầu", 
                "Khám theo dõi", 
                "Xét nghiệm", 
                "Siêu âm", 
                "Tư vấn", 
                "Điều trị",
                "Kết quả điều trị"
            };
            ViewData["RecordTypes"] = recordTypes;
            
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,RecordDate,RecordType,RecordTitle,Description,Results,Diagnosis,Notes,DoctorId,AttachmentPath")] MedicalRecord medicalRecord)
        {
            if (medicalRecord == null || id != medicalRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _medicalRecordService.UpdateMedicalRecordAsync(medicalRecord);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var patients = await _patientService.GetAllPatientsAsync();
            ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", medicalRecord.PatientId);
            var recordTypes = new List<string> 
            { 
                "Khám ban đầu", 
                "Khám theo dõi", 
                "Xét nghiệm", 
                "Siêu âm", 
                "Tư vấn", 
                "Điều trị",
                "Kết quả điều trị"
            };
            ViewData["RecordTypes"] = recordTypes;
            
            var doctors = await _doctorService.GetAllDoctorsAsync();
            ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", medicalRecord.DoctorId);
            
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _medicalRecordService.GetMedicalRecordByIdAsync(id.Value);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _medicalRecordService.DeleteMedicalRecordAsync(id);
                TempData["Success"] = "Hồ sơ y tế đã được xóa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa hồ sơ y tế: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: MedicalRecords/PatientHistory/5
        public async Task<IActionResult> PatientHistory(int? id)
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

            var medicalRecords = await _medicalRecordService.GetMedicalRecordsByPatientAsync(id.Value);
            
            // Truyền dữ liệu qua ViewData để view có thể sử dụng
            ViewData["PatientName"] = patient.FullName;
            ViewData["PatientId"] = patient.Id;
            ViewBag.Patient = patient;

            return View(medicalRecords.OrderByDescending(mr => mr.RecordDate));
        }

        // GET: MedicalRecords/Critical
        public async Task<IActionResult> Critical()
        {
            var criticalRecords = await _medicalRecordService.GetCriticalMedicalRecordsAsync();
            return View(criticalRecords);
        }

        // GET: MedicalRecords/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalRecords = await _medicalRecordService.GetTotalMedicalRecordsCountAsync();
            var recordsByType = await _medicalRecordService.GetMedicalRecordsByTypeStatisticsAsync();

            ViewBag.TotalRecords = totalRecords;
            ViewBag.RecordsByType = recordsByType;

            return View();
        }
    }
} 