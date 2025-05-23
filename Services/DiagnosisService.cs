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
                RiskLevel.Low => $"Nguy cơ hiếm muộn ở mức thấp. Tuy nhiên, nếu sau 12 tháng (hoặc 6 tháng nếu trên 35 tuổi) vẫn chưa có thai, nên tham khảo ý kiến bác sĩ.",
                RiskLevel.Medium => $"Có một số yếu tố nguy cơ hiếm muộn. Khuyến nghị tham khảo ý kiến bác sĩ chuyên khoa để được tư vấn và kiểm tra thêm.",
                RiskLevel.High => $"Nguy cơ hiếm muộn ở mức cao. Nên đến cơ sở y tế chuyên khoa để được kiểm tra và điều trị kịp thời.",
                RiskLevel.VeryHigh => $"Nguy cơ hiếm muộn rất cao. Cần đến cơ sở y tế chuyên khoa ngay để được chẩn đoán và điều trị chuyên sâu.",
                _ => "Không thể đánh giá chính xác. Vui lòng tham khảo ý kiến bác sĩ."
            };
        }

        private string GenerateRecommendations(RiskLevel riskLevel, Gender gender)
        {
            var commonRecommendations = new List<string>
            {
                "Duy trì chế độ ăn uống lành mạnh, cân bằng dinh dưỡng",
                "Tập thể dục đều đặn, tránh stress",
                "Hạn chế rượu bia, không hút thuốc lá",
                "Quan hệ tình dục đều đặn vào thời kỳ rụng trứng"
            };

            var specificRecommendations = riskLevel switch
            {
                RiskLevel.Medium or RiskLevel.High or RiskLevel.VeryHigh => new List<string>
                {
                    "Khám sức khỏe sinh sản định kỳ",
                    "Xét nghiệm hormone sinh sản",
                    "Siêu âm kiểm tra cơ quan sinh sản",
                    "Tham khảo ý kiến bác sĩ chuyên khoa hiếm muộn"
                },
                _ => new List<string>()
            };

            if (gender == Gender.Female && riskLevel >= RiskLevel.Medium)
            {
                specificRecommendations.Add("Theo dõi chu kỳ kinh nguyệt và thời kỳ rụng trứng");
            }

            var allRecommendations = commonRecommendations.Concat(specificRecommendations);
            return string.Join("\n• ", allRecommendations.Select((r, i) => $"{i + 1}. {r}"));
        }
    }
} 