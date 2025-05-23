using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class Patient
    {
        public int Id { get; set; }
        
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
        
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<DiagnosisResult> DiagnosisResults { get; set; } = new List<DiagnosisResult>();
    }

    public enum Gender
    {
        [Display(Name = "Nam")]
        Male = 1,
        [Display(Name = "Nữ")]
        Female = 2
    }
} 