using Microsoft.AspNetCore.Mvc;
using InfertilityApp.Models;
using InfertilityApp.Models.ViewModels;
using InfertilityApp.Services;

namespace InfertilityApp.Controllers
{
    public class DiagnosisController : Controller
    {
        private readonly IDiagnosisService _diagnosisService;

        public DiagnosisController(IDiagnosisService diagnosisService)
        {
            _diagnosisService = diagnosisService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            var viewModel = new DiagnosisViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Questions(DiagnosisViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Start", model);
            }

            var questions = await _diagnosisService.GetQuestionsForGenderAsync(model.Gender);
            model.Questions = questions;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessDiagnosis(DiagnosisViewModel model)
        {
            try
            {
                var patient = new Patient
                {
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Address = model.Address
                };

                var result = await _diagnosisService.ProcessDiagnosisAsync(
                    patient, 
                    model.Answers, 
                    model.MultipleChoiceAnswers);

                return RedirectToAction("Result", new { id = result.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi xử lý chẩn đoán. Vui lòng thử lại.");
                
                var questions = await _diagnosisService.GetQuestionsForGenderAsync(model.Gender);
                model.Questions = questions;
                
                return View("Questions", model);
            }
        }

        public async Task<IActionResult> Result(int id)
        {
            var result = await _diagnosisService.GetDiagnosisResultAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            var viewModel = new DiagnosisResultViewModel
            {
                Patient = result.Patient,
                Result = result,
                Answers = result.Answers.ToList()
            };

            // Create question texts dictionary for display
            foreach (var answer in result.Answers)
            {
                viewModel.QuestionTexts[answer.QuestionId] = answer.Question.QuestionText;
            }

            return View(viewModel);
        }

        public async Task<IActionResult> History(int patientId)
        {
            var history = await _diagnosisService.GetPatientDiagnosisHistoryAsync(patientId);
            return View(history);
        }
    }
} 