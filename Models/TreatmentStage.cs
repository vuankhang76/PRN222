using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class TreatmentStage
    {
        public int Id { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        [ForeignKey("TreatmentId")]
        public virtual Treatment? Treatment { get; set; }

        [Required]
        [StringLength(100)]
        public string StageName { get; set; } = null!;

        [Required]
        public int StageOrder { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } // Planned, In Progress, Completed, Cancelled

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? Results { get; set; }

        // Navigation properties
        public virtual ICollection<Procedure>? Procedures { get; set; }
    }
} 