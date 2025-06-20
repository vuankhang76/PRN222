using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Specialization { get; set; } = null!;

        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        [StringLength(100)]
        public string? Qualifications { get; set; }

        [StringLength(500)]
        public string? Biography { get; set; }

        // Navigation properties
        public virtual ICollection<Treatment>? Treatments { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }
    }
} 