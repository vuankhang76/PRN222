using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class Medication
    {
        public int Id { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        [ForeignKey("TreatmentId")]
        public virtual Treatment? Treatment { get; set; }

        [Required]
        [StringLength(100)]
        public string MedicationName { get; set; } = null!;

        [StringLength(100)]
        public string? Dosage { get; set; }

        [StringLength(200)]
        public string? Instructions { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string? Frequency { get; set; } // Daily, Twice a day, etc.

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } // Active, Completed, Discontinued
    }
} 