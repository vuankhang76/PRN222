using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfertilityApp.Models
{
    public class TreatmentOutcome
    {
        public int Id { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        [ForeignKey("TreatmentId")]
        public virtual Treatment? Treatment { get; set; }

        [Required]
        [StringLength(50)]
        public string OutcomeType { get; set; } = null!; // Pregnancy, Birth, Miscarriage, Failed, Ongoing

        [Required]
        public DateTime OutcomeDate { get; set; }

        public bool IsPregnant { get; set; } = false;

        public DateTime? PregnancyTestDate { get; set; }

        [StringLength(50)]
        public string? PregnancyTestResult { get; set; } // Positive, Negative, Beta HCG value

        public DateTime? ExpectedDueDate { get; set; }

        public DateTime? ActualBirthDate { get; set; }

        public int? NumberOfBabies { get; set; } // Số con sinh ra

        [StringLength(50)]
        public string? DeliveryType { get; set; } // Natural, C-Section

        [StringLength(50)]
        public string? BabyGender { get; set; } // Male, Female, Multiple

        [Column(TypeName = "decimal(5,2)")]
        public decimal? BirthWeight { get; set; } // Cân nặng khi sinh (kg)

        [StringLength(50)]
        public string? BabyHealth { get; set; } // Healthy, Complications

        [StringLength(1000)]
        public string? Complications { get; set; } // Biến chứng

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? FinalStatus { get; set; } // Success, Unsuccessful, Incomplete

        public int? DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
} 