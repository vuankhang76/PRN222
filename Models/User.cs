using System;
using System.ComponentModel.DataAnnotations;

namespace InfertilityApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string FullName { get; set; } = null!;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = null!; // Admin, Doctor, Nurse, Receptionist

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        // For doctor users, link to their Doctor profile
        public int? DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }
    }
} 