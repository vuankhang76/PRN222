using Microsoft.EntityFrameworkCore;
using System;

namespace InfertilityApp.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<TreatmentStage> TreatmentStages { get; set; }
        public DbSet<Procedure> Procedures { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; }
        public DbSet<TreatmentOutcome> TreatmentOutcomes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between Patient and Partner
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Partner)
                .WithOne(p => p.Patient)
                .HasForeignKey<Partner>(p => p.PatientId);

            // Configure one-to-many relationship between Patient and Treatment
            modelBuilder.Entity<Treatment>()
                .HasOne(t => t.Patient)
                .WithMany(p => p.Treatments)
                .HasForeignKey(t => t.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationship between Doctor and Treatment
            modelBuilder.Entity<Treatment>()
                .HasOne(t => t.Doctor)
                .WithMany(d => d.Treatments)
                .HasForeignKey(t => t.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationship between Treatment and TreatmentStage
            modelBuilder.Entity<TreatmentStage>()
                .HasOne(ts => ts.Treatment)
                .WithMany(t => t.TreatmentStages)
                .HasForeignKey(ts => ts.TreatmentId);

            // Configure one-to-many relationship between TreatmentStage and Procedure
            modelBuilder.Entity<Procedure>()
                .HasOne(p => p.TreatmentStage)
                .WithMany(ts => ts.Procedures)
                .HasForeignKey(p => p.TreatmentStageId);

            // Configure one-to-many relationship between Treatment and Medication
            modelBuilder.Entity<Medication>()
                .HasOne(m => m.Treatment)
                .WithMany(t => t.Medications)
                .HasForeignKey(m => m.TreatmentId);

            // Configure one-to-many relationship between Patient and MedicalRecord
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(mr => mr.PatientId);

            // Configure one-to-many relationship between Doctor and MedicalRecord
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Doctor)
                .WithMany()
                .HasForeignKey(mr => mr.DoctorId)
                .IsRequired(false);

            // Configure one-to-many relationship between Patient and Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId);

            // Configure one-to-many relationship between Doctor and Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId);

            // Configure relationships for TestResult
            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Patient)
                .WithMany()
                .HasForeignKey(tr => tr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Partner)
                .WithMany()
                .HasForeignKey(tr => tr.PartnerId)
                .IsRequired(false);

            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Doctor)
                .WithMany()
                .HasForeignKey(tr => tr.DoctorId)
                .IsRequired(false);

            // Configure relationship for MedicationSchedule
            modelBuilder.Entity<MedicationSchedule>()
                .HasOne(ms => ms.Medication)
                .WithMany()
                .HasForeignKey(ms => ms.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship for TreatmentOutcome
            modelBuilder.Entity<TreatmentOutcome>()
                .HasOne(to => to.Treatment)
                .WithMany()
                .HasForeignKey(to => to.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TreatmentOutcome>()
                .HasOne(to => to.Doctor)
                .WithMany()
                .HasForeignKey(to => to.DoctorId)
                .IsRequired(false);

            // Configure one-to-many relationship between Treatment and Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Treatment)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TreatmentId)
                .IsRequired(false);
        }
    }
} 