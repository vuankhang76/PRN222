using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models.ViewModels
{
    public class DiagnosisViewModel
    {
        // Patient Information
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public Gender Gender { get; set; }
        
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
        
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }
        
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }
        
        // Questions and Answers
        public List<DiagnosisQuestion> Questions { get; set; } = new List<DiagnosisQuestion>();
        public Dictionary<int, string> Answers { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, List<int>> MultipleChoiceAnswers { get; set; } = new Dictionary<int, List<int>>();
    }
} 