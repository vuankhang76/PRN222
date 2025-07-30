using Microsoft.AspNetCore.Mvc;
using InfertilityApp.Models;
using InfertilityApp.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InfertilityApp.Controllers
{
    public class TestResultsController : Controller
    {
        private readonly ITestResultService _testResultService;
        private readonly IPatientService _patientService;
        private readonly IPartnerService _partnerService;
        private readonly IDoctorService _doctorService;

        public TestResultsController(
            ITestResultService testResultService,
            IPatientService patientService,
            IPartnerService partnerService,
            IDoctorService doctorService)
        {
            _testResultService = testResultService;
            _patientService = patientService;
            _partnerService = partnerService;
            _doctorService = doctorService;
        }

        // GET: TestResults
        public async Task<IActionResult> Index(int? patientId, int? treatmentId)
        {
            try
            {
                var testResults = await _testResultService.GetAllAsync();
                
                if (patientId.HasValue)
                {
                    testResults = testResults.Where(tr => tr.PatientId == patientId.Value);
                    var patient = await _patientService.GetPatientByIdAsync(patientId.Value);
                    ViewData["PatientName"] = patient?.FullName ?? "Không xác định";
                    ViewData["PatientId"] = patientId.Value;
                }

                if (treatmentId.HasValue)
                {
                    ViewData["TreatmentId"] = treatmentId.Value;
                }

                return View(testResults.OrderByDescending(tr => tr.TestDate));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách kết quả xét nghiệm: " + ex.Message;
                return View(new List<TestResult>());
            }
        }

        // GET: TestResults/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var testResult = await _testResultService.GetByIdAsync(id);
                if (testResult == null)
                {
                    TempData["Error"] = "Không tìm thấy kết quả xét nghiệm.";
                    return RedirectToAction(nameof(Index));
                }

                return View(testResult);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin kết quả xét nghiệm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: TestResults/Create
        public async Task<IActionResult> Create(int? patientId, int? partnerId)
        {
            try
            {
                await LoadDropdownData();
                
                var model = new TestResult();
                if (patientId.HasValue)
                {
                    model.PatientId = patientId.Value;
                    var patient = await _patientService.GetPatientByIdAsync(patientId.Value);
                    ViewData["PatientName"] = patient?.FullName;
                }
                if (partnerId.HasValue)
                {
                    model.PartnerId = partnerId.Value;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải trang thêm mới: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: TestResults/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestResult testResult)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _testResultService.AddAsync(testResult);
                    TempData["Success"] = "Thêm kết quả xét nghiệm thành công!";
                    return RedirectToAction(nameof(Index), new { patientId = testResult.PatientId });
                }

                await LoadDropdownData();
                return View(testResult);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm kết quả xét nghiệm: " + ex.Message;
                await LoadDropdownData();
                return View(testResult);
            }
        }

        // GET: TestResults/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var testResult = await _testResultService.GetByIdAsync(id);
                if (testResult == null)
                {
                    TempData["Error"] = "Không tìm thấy kết quả xét nghiệm.";
                    return RedirectToAction(nameof(Index));
                }

                await LoadDropdownData();
                return View(testResult);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: TestResults/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TestResult testResult)
        {
            if (id != testResult.Id)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _testResultService.UpdateAsync(testResult);
                    TempData["Success"] = "Cập nhật kết quả xét nghiệm thành công!";
                    return RedirectToAction(nameof(Index), new { patientId = testResult.PatientId });
                }

                await LoadDropdownData();
                return View(testResult);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật kết quả xét nghiệm: " + ex.Message;
                await LoadDropdownData();
                return View(testResult);
            }
        }

        // GET: TestResults/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var testResult = await _testResultService.GetByIdAsync(id);
                if (testResult == null)
                {
                    TempData["Error"] = "Không tìm thấy kết quả xét nghiệm.";
                    return RedirectToAction(nameof(Index));
                }

                return View(testResult);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin xóa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: TestResults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var testResult = await _testResultService.GetByIdAsync(id);
                if (testResult != null)
                {
                    var patientId = testResult.PatientId;
                    await _testResultService.DeleteAsync(id);
                    TempData["Success"] = "Xóa kết quả xét nghiệm thành công!";
                    return RedirectToAction(nameof(Index), new { patientId = patientId });
                }

                TempData["Error"] = "Không tìm thấy kết quả xét nghiệm để xóa.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa kết quả xét nghiệm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadDropdownData()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var partners = await _partnerService.GetAllPartnersAsync();
                var doctors = await _doctorService.GetAllDoctorsAsync();

                ViewBag.PatientList = new SelectList(patients, "Id", "FullName");
                ViewBag.PartnerList = new SelectList(partners, "Id", "FullName");
                ViewBag.DoctorList = new SelectList(doctors, "Id", "FullName");

                ViewBag.TestTypes = new SelectList(new[]
                {
                    new { Value = "Hormone", Text = "Xét nghiệm hormone" },
                    new { Value = "Semen Analysis", Text = "Phân tích tinh dịch" },
                    new { Value = "HSG", Text = "Chụp tử cung vòi trứng" },
                    new { Value = "Blood Test", Text = "Xét nghiệm máu" },
                    new { Value = "Ultrasound", Text = "Siêu âm" },
                    new { Value = "Genetic", Text = "Xét nghiệm di truyền" },
                    new { Value = "Infection", Text = "Xét nghiệm nhiễm trùng" },
                    new { Value = "Other", Text = "Khác" }
                }, "Value", "Text");

                ViewBag.StatusList = new SelectList(new[]
                {
                    new { Value = "Normal", Text = "Bình thường" },
                    new { Value = "Abnormal", Text = "Bất thường" },
                    new { Value = "Borderline", Text = "Biên giới" },
                    new { Value = "Critical", Text = "Nguy hiểm" }
                }, "Value", "Text");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu dropdown: " + ex.Message;
            }
        }
    }
}
