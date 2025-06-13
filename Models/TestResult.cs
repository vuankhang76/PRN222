using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class TestResult
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        public int? PartnerId { get; set; }

        [ForeignKey("PartnerId")]
        public virtual Partner? Partner { get; set; }

        [Required]
        [StringLength(100)]
        public string TestType { get; set; } = null!; // Hormone, Semen Analysis, HSG, Blood Test, etc.

        [Required]
        [StringLength(200)]
        public string TestName { get; set; } = null!;

        [Required]
        public DateTime TestDate { get; set; }

        [Required]
        [StringLength(2000)]
        public string Results { get; set; } = null!;

        [StringLength(500)]
        public string? ReferenceRange { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } // Normal, Abnormal, Borderline, Critical

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? TestingLab { get; set; } // Phòng xét nghiệm

        public int? DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        [StringLength(255)]
        public string? AttachmentPath { get; set; } // Đường dẫn file kết quả

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 