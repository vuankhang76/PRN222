<<<<<<< HEAD
=======
using System;
using System.Collections.Generic;
>>>>>>> fc5dcc23dac80d2b1678e1d4edb23fa04dc32521
using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class Patient
    {
        public int Id { get; set; }
<<<<<<< HEAD
        
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
=======

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        [StringLength(1000)]
        public string? MedicalHistory { get; set; }

        [StringLength(500)]
        public string? AllergiesInfo { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Partner? Partner { get; set; }
        public virtual ICollection<Treatment>? Treatments { get; set; }
        public virtual ICollection<MedicalRecord>? MedicalRecords { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }
>>>>>>> fc5dcc23dac80d2b1678e1d4edb23fa04dc32521
    }
} 