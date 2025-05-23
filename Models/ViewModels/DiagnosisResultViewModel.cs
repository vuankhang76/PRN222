namespace InfertilityApp.Models.ViewModels
{
    public class DiagnosisResultViewModel
    {
        public Patient Patient { get; set; } = null!;
        public DiagnosisResult Result { get; set; } = null!;
        public List<DiagnosisAnswer> Answers { get; set; } = new List<DiagnosisAnswer>();
        public Dictionary<int, string> QuestionTexts { get; set; } = new Dictionary<int, string>();
    }
} 