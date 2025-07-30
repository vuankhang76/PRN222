using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class Patient
    {
        public int Id { get; set; }

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
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Partner? Partner { get; set; }
        public virtual ICollection<Treatment>? Treatments { get; set; }
        public virtual ICollection<MedicalRecord>? MedicalRecords { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }
    }
} 