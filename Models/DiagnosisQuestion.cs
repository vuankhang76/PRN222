using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class DiagnosisQuestion
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Câu hỏi")]
        public string QuestionText { get; set; } = string.Empty;
        
        [Display(Name = "Loại câu hỏi")]
        public QuestionType Type { get; set; }
        
        [Display(Name = "Áp dụng cho giới tính")]
        public Gender? ApplicableGender { get; set; }
        
        [Display(Name = "Thứ tự")]
        public int Order { get; set; }
        
        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public virtual ICollection<DiagnosisAnswer> Answers { get; set; } = new List<DiagnosisAnswer>();
    }

    public enum QuestionType
    {
        [Display(Name = "Một lựa chọn")]
        SingleChoice = 1,
        [Display(Name = "Nhiều lựa chọn")]
        MultipleChoice = 2,
        [Display(Name = "Văn bản")]
        Text = 3,
        [Display(Name = "Số")]
        Number = 4,
        [Display(Name = "Ngày tháng")]
        Date = 5
    }
} 