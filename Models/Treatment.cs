using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class Treatment
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        public int? AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string TreatmentType { get; set; } = null!; // IVF, IUI, etc.

        [Required]
        [StringLength(100)]
        public string TreatmentName { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } // Possible values: In Progress, Completed, Paused, Cancelled

        // Định nghĩa các trạng thái chuẩn
        public static class TreatmentStatus
        {
            public const string InProgress = "In Progress";
            public const string Completed = "Completed";
            public const string Paused = "Paused";
            public const string Cancelled = "Cancelled";
        }

        [StringLength(50)]
        public string? Outcome { get; set; } // Successful, Unsuccessful, Ongoing

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ICollection<TreatmentStage>? TreatmentStages { get; set; }
        public virtual ICollection<Medication>? Medications { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }
    }
} 