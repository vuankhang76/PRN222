using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class DiagnosisResult
    {
        public int Id { get; set; }
        
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;
        
        [Display(Name = "Tổng điểm")]
        public int TotalScore { get; set; }
        
        [Display(Name = "Kết quả chẩn đoán")]
        public string DiagnosisText { get; set; } = string.Empty;
        
        [Display(Name = "Mức độ nguy cơ")]
        public RiskLevel RiskLevel { get; set; }
        
        [Display(Name = "Khuyến nghị")]
        public string Recommendations { get; set; } = string.Empty;
        
        [Display(Name = "Ngày chẩn đoán")]
        public DateTime DiagnosisDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<DiagnosisAnswer> Answers { get; set; } = new List<DiagnosisAnswer>();
    }

    public enum RiskLevel
    {
        [Display(Name = "Thấp")]
        Low = 1,
        [Display(Name = "Trung bình")]
        Medium = 2,
        [Display(Name = "Cao")]
        High = 3,
        [Display(Name = "Rất cao")]
        VeryHigh = 4
    }
} 