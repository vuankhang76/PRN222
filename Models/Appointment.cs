using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class Appointment
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

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        public int Duration { get; set; } // In minutes

        [StringLength(500)]
        public string? Purpose { get; set; } // Mục đích cuộc hẹn

        [StringLength(50)]
        public string? Status { get; set; } // Scheduled, Completed, Cancelled, Rescheduled

        [StringLength(100)]
        public string? AppointmentType { get; set; } // Consultation, Follow-up, Procedure, etc.

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? TreatmentId { get; set; }

        [ForeignKey("TreatmentId")]
        public virtual Treatment? Treatment { get; set; }
    }
} 