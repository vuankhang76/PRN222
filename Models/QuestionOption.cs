using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Nội dung lựa chọn")]
        public string OptionText { get; set; } = string.Empty;
        
        [Display(Name = "Điểm số")]
        public int Score { get; set; } = 0;
        
        [Display(Name = "Thứ tự")]
        public int Order { get; set; }
        
        public int QuestionId { get; set; }
        public virtual DiagnosisQuestion Question { get; set; } = null!;
    }
} 