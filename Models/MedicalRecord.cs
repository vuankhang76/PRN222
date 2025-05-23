using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [Required]
        public DateTime RecordDate { get; set; }

        [Required]
        [StringLength(100)]
        public string RecordType { get; set; } = null!; // Consultation, Test Result, Ultrasound, etc.

        [Required]
        [StringLength(200)]
        public string RecordTitle { get; set; } = null!; // Tiêu đề hồ sơ

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Results { get; set; } // Kết quả xét nghiệm, siêu âm, v.v.

        [StringLength(1000)]
        public string? Diagnosis { get; set; }

        [StringLength(1000)]
        public string? Recommendations { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(255)]
        public string? FileAttachment { get; set; } // Path to attached file (legacy)

        [StringLength(255)]
        public string? AttachmentPath { get; set; } // Path to attached file (new)

        public int? DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
    }
} 