using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class MedicationSchedule
    {
        public int Id { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [ForeignKey("MedicationId")]
        public virtual Medication? Medication { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public TimeSpan ScheduledTime { get; set; }

        [Required]
        [StringLength(100)]
        public string Dosage { get; set; } = null!; // 1 viên, 2ml, etc.

        public bool IsTaken { get; set; } = false;

        public DateTime? ActualTakenTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } // Scheduled, Taken, Missed, Cancelled

        [StringLength(200)]
        public string? Instructions { get; set; } // Uống trước/sau ăn, etc.

        public bool IsReminded { get; set; } = false; // Đã nhắc nhở chưa

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
} 