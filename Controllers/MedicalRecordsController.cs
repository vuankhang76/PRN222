using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InfertilityApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace InfertilityApp.Controllers
{
    public class MedicalRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MedicalRecordsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index(string searchString, DateTime? fromDate, DateTime? toDate, int? patientId, int? doctorId, string recordType)
        {
            var medicalRecords = _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .AsQueryable();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                medicalRecords = medicalRecords.Where(m => 
                    (m.RecordTitle != null && m.RecordTitle.Contains(searchString)) || 
                    (m.Description != null && m.Description.Contains(searchString)) ||
                    m.Patient.FullName.Contains(searchString) ||
                    (m.Doctor != null && m.Doctor.FullName.Contains(searchString)));
            }

            // Lọc theo ngày
            if (fromDate.HasValue)
            {
                medicalRecords = medicalRecords.Where(m => m.RecordDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                medicalRecords = medicalRecords.Where(m => m.RecordDate <= toDate.Value);
            }

            // Lọc theo bệnh nhân
            if (patientId.HasValue)
            {
                medicalRecords = medicalRecords.Where(m => m.PatientId == patientId.Value);
            }

            // Lọc theo bác sĩ
            if (doctorId.HasValue)
            {
                medicalRecords = medicalRecords.Where(m => m.DoctorId == doctorId.Value);
            }

            // Lọc theo loại hồ sơ
            if (!string.IsNullOrEmpty(recordType))
            {
                medicalRecords = medicalRecords.Where(m => m.RecordType == recordType);
            }

            // Chuẩn bị dữ liệu cho dropdown
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName");
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName");
            ViewData["RecordTypes"] = new List<string> { 
                "Xét nghiệm máu", "Siêu âm", "Xét nghiệm nội tiết", 
                "Chẩn đoán hình ảnh", "Tinh dịch đồ", "Kết quả phôi", "Khác" 
            };

            // Lưu các tham số lọc để hiển thị lại khi refresh trang
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentPatientId"] = patientId;
            ViewData["CurrentDoctorId"] = doctorId;
            ViewData["CurrentRecordType"] = recordType;

            return View(await medicalRecords.OrderByDescending(m => m.RecordDate).ToListAsync());
        }

        // GET: MedicalRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // GET: MedicalRecords/Create
        public IActionResult Create(int? patientId)
        {
            // Nếu có patientId, tự động chọn bệnh nhân
            if (patientId.HasValue)
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", patientId);
            }
            else
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName");
            }
            
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName");
            ViewData["RecordTypes"] = new List<string> { 
                "Xét nghiệm máu", "Siêu âm", "Xét nghiệm nội tiết", 
                "Chẩn đoán hình ảnh", "Tinh dịch đồ", "Kết quả phôi", "Khác" 
            };
            
            // Thiết lập giá trị mặc định
            var medicalRecord = new MedicalRecord
            {
                RecordDate = DateTime.Today,
                PatientId = patientId.HasValue ? patientId.Value : 0
            };
            
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,RecordDate,RecordType,RecordTitle,Description,Results")] MedicalRecord medicalRecord, IFormFile attachment)
        {
            if (ModelState.IsValid)
            {
                // Xử lý tệp đính kèm nếu có
                if (attachment != null && attachment.Length > 0)
                {
                    // Tạo thư mục lưu trữ nếu chưa tồn tại
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "medical_records");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file duy nhất
                    string uniqueFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{attachment.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachment.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn file vào cơ sở dữ liệu
                    medicalRecord.AttachmentPath = $"/uploads/medical_records/{uniqueFileName}";
                }

                _context.Add(medicalRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", medicalRecord.PatientId);
            ViewData["RecordTypes"] = new List<string> { 
                "Xét nghiệm máu", "Siêu âm", "Xét nghiệm nội tiết", 
                "Chẩn đoán hình ảnh", "Tinh dịch đồ", "Kết quả phôi", "Khác" 
            };
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", medicalRecord.PatientId);
            ViewData["RecordTypes"] = new List<string> { 
                "Xét nghiệm máu", "Siêu âm", "Xét nghiệm nội tiết", 
                "Chẩn đoán hình ảnh", "Tinh dịch đồ", "Kết quả phôi", "Khác" 
            };
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,RecordDate,RecordType,RecordTitle,Description,Results,AttachmentPath")] MedicalRecord medicalRecord, IFormFile attachment)
        {
            if (id != medicalRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý tệp đính kèm mới nếu có
                    if (attachment != null && attachment.Length > 0)
                    {
                        // Tạo thư mục lưu trữ nếu chưa tồn tại
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "medical_records");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Tạo tên file duy nhất
                        string uniqueFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{attachment.FileName}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Lưu file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await attachment.CopyToAsync(fileStream);
                        }

                        // Xóa file cũ nếu có
                        if (!string.IsNullOrEmpty(medicalRecord.AttachmentPath))
                        {
                            string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, medicalRecord.AttachmentPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Lưu đường dẫn file mới vào cơ sở dữ liệu
                        medicalRecord.AttachmentPath = $"/uploads/medical_records/{uniqueFileName}";
                    }

                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalRecordExists(medicalRecord.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "FullName", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "FullName", medicalRecord.PatientId);
            ViewData["RecordTypes"] = new List<string> { 
                "Xét nghiệm máu", "Siêu âm", "Xét nghiệm nội tiết", 
                "Chẩn đoán hình ảnh", "Tinh dịch đồ", "Kết quả phôi", "Khác" 
            };
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord != null)
            {
                // Xóa file đính kèm nếu có
                if (!string.IsNullOrEmpty(medicalRecord.AttachmentPath))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, medicalRecord.AttachmentPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: MedicalRecords/PatientHistory/5
        public async Task<IActionResult> PatientHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            var medicalRecords = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Where(m => m.PatientId == id)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();

            ViewData["PatientName"] = patient.FullName;
            ViewData["PatientId"] = patient.Id;
            
            return View(medicalRecords);
        }

        // GET: MedicalRecords/DownloadAttachment/5
        public async Task<IActionResult> DownloadAttachment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null || string.IsNullOrEmpty(medicalRecord.AttachmentPath))
            {
                return NotFound();
            }

            string filePath = Path.Combine(_hostEnvironment.WebRootPath, medicalRecord.AttachmentPath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            string fileName = Path.GetFileName(filePath);
            return File(memory, GetContentType(filePath), fileName);
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                {".pdf", "application/pdf"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".png", "image/png"},
                {".doc", "application/msword"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".txt", "text/plain"}
            };
            
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private bool MedicalRecordExists(int id)
        {
            return _context.MedicalRecords.Any(e => e.Id == id);
        }
    }
} 