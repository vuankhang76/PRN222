using InfertilityApp.Data;
using InfertilityApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InfertilityApp.Services
{
    public interface IDiagnosisService
    {
        Task<List<DiagnosisQuestion>> GetQuestionsForGenderAsync(Gender gender);
        Task<DiagnosisResult> ProcessDiagnosisAsync(Patient patient, Dictionary<int, string> answers, Dictionary<int, List<int>> multipleChoiceAnswers);
        Task<DiagnosisResult?> GetDiagnosisResultAsync(int id);
        Task<List<DiagnosisResult>> GetPatientDiagnosisHistoryAsync(int patientId);
    }

    public class DiagnosisService : IDiagnosisService
    {
        private readonly ApplicationDbContext _context;

        public DiagnosisService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DiagnosisQuestion>> GetQuestionsForGenderAsync(Gender gender)
        {
            return await _context.DiagnosisQuestions
                .Include(q => q.Options.OrderBy(o => o.Order))
                .Where(q => q.IsActive && (q.ApplicableGender == null || q.ApplicableGender == gender))
                .OrderBy(q => q.Order)
                .ToListAsync();
        }

        public async Task<DiagnosisResult> ProcessDiagnosisAsync(Patient patient, Dictionary<int, string> answers, Dictionary<int, List<int>> multipleChoiceAnswers)
        {
            // Save patient if new
            if (patient.Id == 0)
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            var questions = await _context.DiagnosisQuestions
                .Include(q => q.Options)
                .ToListAsync();

            var diagnosisResult = new DiagnosisResult
            {
                PatientId = patient.Id,
                DiagnosisDate = DateTime.Now
            };

            int totalScore = 0;
            var diagnosisAnswers = new List<DiagnosisAnswer>();

            // Process answers
            foreach (var answer in answers)
            {
                var question = questions.FirstOrDefault(q => q.Id == answer.Key);
                if (question == null) continue;

                var diagnosisAnswer = new DiagnosisAnswer
                {
                    QuestionId = answer.Key,
                    AnswerText = answer.Value
                };

                int score = 0;

                if (question.Type == QuestionType.SingleChoice)
                {
                    if (int.TryParse(answer.Value, out int optionId))
                    {
                        var option = question.Options.FirstOrDefault(o => o.Id == optionId);
                        if (option != null)
                        {
                            score = option.Score;
                            diagnosisAnswer.AnswerText = option.OptionText;
                        }
                    }
                }
                else if (question.Type == QuestionType.Number)
                {
                    if (float.TryParse(answer.Value, out float numValue))
                    {
                        score = CalculateNumericScore(question.Id, numValue);
                    }
                }

                diagnosisAnswer.Score = score;
                totalScore += score;
                diagnosisAnswers.Add(diagnosisAnswer);
            }

            // Process multiple choice answers
            foreach (var multiAnswer in multipleChoiceAnswers)
            {
                var question = questions.FirstOrDefault(q => q.Id == multiAnswer.Key);
                if (question == null) continue;

                var selectedOptions = question.Options.Where(o => multiAnswer.Value.Contains(o.Id)).ToList();
                var score = selectedOptions.Sum(o => o.Score);
                var answerText = string.Join(", ", selectedOptions.Select(o => o.OptionText));

                var diagnosisAnswer = new DiagnosisAnswer
                {
                    QuestionId = multiAnswer.Key,
                    AnswerText = answerText,
                    Score = score,
                    SelectedOptionIds = string.Join(",", multiAnswer.Value)
                };

                totalScore += score;
                diagnosisAnswers.Add(diagnosisAnswer);
            }

            diagnosisResult.TotalScore = totalScore;
            diagnosisResult.RiskLevel = DetermineRiskLevel(totalScore);
            diagnosisResult.DiagnosisText = GenerateDiagnosisText(diagnosisResult.RiskLevel, patient.Gender);
            diagnosisResult.Recommendations = GenerateRecommendations(diagnosisResult.RiskLevel, patient.Gender);

            _context.DiagnosisResults.Add(diagnosisResult);
            await _context.SaveChangesAsync();

            // Add answers with the diagnosis result ID
            foreach (var answer in diagnosisAnswers)
            {
                answer.DiagnosisResultId = diagnosisResult.Id;
            }

            _context.DiagnosisAnswers.AddRange(diagnosisAnswers);
            await _context.SaveChangesAsync();

            return diagnosisResult;
        }

        public async Task<DiagnosisResult?> GetDiagnosisResultAsync(int id)
        {
            return await _context.DiagnosisResults
                .Include(r => r.Patient)
                .Include(r => r.Answers)
                .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<DiagnosisResult>> GetPatientDiagnosisHistoryAsync(int patientId)
        {
            return await _context.DiagnosisResults
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.DiagnosisDate)
                .ToListAsync();
        }

        private int CalculateNumericScore(int questionId, float value)
        {
            return questionId switch
            {
                2 => value switch // Age
                {
                    < 25 => 1,
                    >= 25 and < 30 => 2,
                    >= 30 and < 35 => 3,
                    >= 35 and < 40 => 5,
                    >= 40 => 8,
                    _ => 0
                },
                6 => value switch // BMI
                {
                    < 18.5f => 3,
                    >= 18.5f and < 25 => 0,
                    >= 25 and < 30 => 2,
                    >= 30 => 5,
                    _ => 0
                },
                _ => 0
            };
        }

        private RiskLevel DetermineRiskLevel(int totalScore)
        {
            return totalScore switch
            {
                <= 5 => RiskLevel.Low,
                <= 15 => RiskLevel.Medium,
                <= 25 => RiskLevel.High,
                _ => RiskLevel.VeryHigh
            };
        }

        private string GenerateDiagnosisText(RiskLevel riskLevel, Gender gender)
        {
            var genderText = gender == Gender.Female ? "nữ" : "nam";
            
            return riskLevel switch
            {
                RiskLevel.Low => $"Hồ sơ cho thấy các yếu tố nguy cơ ở mức thấp. Tiếp tục theo dõi và tuân thủ kế hoạch điều trị (nếu có).",
                RiskLevel.Medium => $"Hồ sơ cho thấy một số yếu tố cần lưu ý trong quá trình điều trị. Nên thảo luận kỹ với bác sĩ về kế hoạch theo dõi và các điều chỉnh cần thiết.",
                RiskLevel.High => $"Hồ sơ cho thấy các yếu tố nguy cơ ở mức cao. Cần tuân thủ chặt chẽ kế hoạch điều trị và theo dõi sát sao với bác sĩ chuyên khoa.",
                RiskLevel.VeryHigh => $"Hồ sơ cho thấy các yếu tố nguy cơ ở mức rất cao. Việc theo dõi và quản lý điều trị cần được thực hiện hết sức cẩn trọng dưới sự giám sát y tế chặt chẽ.",
                _ => "Không đủ thông tin để đưa ra đánh giá chi tiết. Vui lòng cập nhật đầy đủ hồ sơ và tham khảo ý kiến bác sĩ."
            };
        }

        private string GenerateRecommendations(RiskLevel riskLevel, Gender gender)
        {
            var commonRecommendations = new List<string>
            {
                "Duy trì chế độ ăn uống lành mạnh, cân bằng dinh dưỡng theo hướng dẫn của bác sĩ.",
                "Tập thể dục đều đặn với cường độ phù hợp, quản lý stress hiệu quả.",
                "Tuân thủ lịch uống thuốc và các chỉ định điều trị khác.",
                "Ghi chép lại các triệu chứng hoặc thay đổi bất thường để thảo luận với bác sĩ.",
                "Tái khám đúng hẹn để bác sĩ theo dõi tiến trình điều trị."
            };

            var specificRecommendations = new List<string>();

            switch (riskLevel)
            {
                case RiskLevel.Low:
                    specificRecommendations.Add("Tiếp tục duy trì lối sống lành mạnh và theo dõi định kỳ.");
                    break;
                case RiskLevel.Medium:
                    specificRecommendations.Add("Thảo luận với bác sĩ về các biện pháp hỗ trợ cụ thể cho tình trạng của bạn.");
                    specificRecommendations.Add("Cân nhắc việc theo dõi một số chỉ số sức khỏe thường xuyên hơn.");
                    break;
                case RiskLevel.High:
                case RiskLevel.VeryHigh:
                    specificRecommendations.Add("Tuyệt đối tuân thủ phác đồ điều trị và hướng dẫn của bác sĩ.");
                    specificRecommendations.Add("Không tự ý thay đổi hoặc ngưng thuốc mà không có chỉ định.");
                    specificRecommendations.Add("Thông báo ngay cho bác sĩ nếu có bất kỳ dấu hiệu bất thường nghiêm trọng nào.");
                    break;
            }
            
            commonRecommendations.AddRange(specificRecommendations);
            return string.Join("\n- ", commonRecommendations.Prepend("- "));
        }
    }
} 