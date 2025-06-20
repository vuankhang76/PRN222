using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class Procedure
    {
        public int Id { get; set; }

        [Required]
        public int TreatmentStageId { get; set; }

        [ForeignKey("TreatmentStageId")]
        public virtual TreatmentStage? TreatmentStage { get; set; }

        [Required]
        [StringLength(100)]
        public string ProcedureName { get; set; } = null!;

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? ActualDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } // Scheduled, Completed, Cancelled

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Results { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
    }
} 