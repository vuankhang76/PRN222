namespace InfertilityApp.Models
{
    public class DiagnosisAnswer
    {
        public int Id { get; set; }
        
        public int DiagnosisResultId { get; set; }
        public virtual DiagnosisResult DiagnosisResult { get; set; } = null!;
        
        public int QuestionId { get; set; }
        public virtual DiagnosisQuestion Question { get; set; } = null!;
        
        public string AnswerText { get; set; } = string.Empty;
        
        public int Score { get; set; } = 0;
        
        // Cho multiple choice questions
        public string? SelectedOptionIds { get; set; }
    }
} 