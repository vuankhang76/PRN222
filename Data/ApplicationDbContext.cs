using Microsoft.EntityFrameworkCore;
using InfertilityApp.Models;

namespace InfertilityApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<DiagnosisQuestion> DiagnosisQuestions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<DiagnosisResult> DiagnosisResults { get; set; }
        public DbSet<DiagnosisAnswer> DiagnosisAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<QuestionOption>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId);

            modelBuilder.Entity<DiagnosisResult>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.DiagnosisResults)
                .HasForeignKey(r => r.PatientId);

            modelBuilder.Entity<DiagnosisAnswer>()
                .HasOne(a => a.DiagnosisResult)
                .WithMany(r => r.Answers)
                .HasForeignKey(a => a.DiagnosisResultId);

            modelBuilder.Entity<DiagnosisAnswer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed questions for infertility diagnosis
            var questions = new List<DiagnosisQuestion>
            {
                new DiagnosisQuestion
                {
                    Id = 1,
                    QuestionText = "Bạn đã bao lâu cố gắng có con?",
                    Type = QuestionType.SingleChoice,
                    Order = 1,
                    IsActive = true
                },
                new DiagnosisQuestion
                {
                    Id = 2,
                    QuestionText = "Tuổi của bạn hiện tại?",
                    Type = QuestionType.Number,
                    Order = 2,
                    IsActive = true
                },
                new DiagnosisQuestion
                {
                    Id = 3,
                    QuestionText = "Chu kỳ kinh nguyệt của bạn có đều đặn không?",
                    Type = QuestionType.SingleChoice,
                    ApplicableGender = Gender.Female,
                    Order = 3,
                    IsActive = true
                },
                new DiagnosisQuestion
                {
                    Id = 4,
                    QuestionText = "Bạn có tiền sử các bệnh phụ khoa không?",
                    Type = QuestionType.MultipleChoice,
                    ApplicableGender = Gender.Female,
                    Order = 4,
                    IsActive = true
                },
                new DiagnosisQuestion
                {
                    Id = 5,
                    QuestionText = "Bạn có hút thuốc lá không?",
                    Type = QuestionType.SingleChoice,
                    Order = 5,
                    IsActive = true
                },
                new DiagnosisQuestion
                {
                    Id = 6,
                    QuestionText = "BMI (chỉ số khối cơ thể) của bạn?",
                    Type = QuestionType.Number,
                    Order = 6,
                    IsActive = true
                },
                new DiagnosisQuestion
                {
                    Id = 7,
                    QuestionText = "Bạn có tiền sử phẫu thuật vùng chậu không?",
                    Type = QuestionType.SingleChoice,
                    Order = 7,
                    IsActive = true
                },
                new DiagnosisQuestion
                {
                    Id = 8,
                    QuestionText = "Tần suất quan hệ tình dục của bạn?",
                    Type = QuestionType.SingleChoice,
                    Order = 8,
                    IsActive = true
                }
            };

            modelBuilder.Entity<DiagnosisQuestion>().HasData(questions);

            // Seed options
            var options = new List<QuestionOption>
            {
                // Question 1 options
                new QuestionOption { Id = 1, QuestionId = 1, OptionText = "Dưới 6 tháng", Score = 1, Order = 1 },
                new QuestionOption { Id = 2, QuestionId = 1, OptionText = "6-12 tháng", Score = 3, Order = 2 },
                new QuestionOption { Id = 3, QuestionId = 1, OptionText = "1-2 năm", Score = 5, Order = 3 },
                new QuestionOption { Id = 4, QuestionId = 1, OptionText = "Trên 2 năm", Score = 8, Order = 4 },

                // Question 3 options
                new QuestionOption { Id = 5, QuestionId = 3, OptionText = "Rất đều đặn (28-30 ngày)", Score = 1, Order = 1 },
                new QuestionOption { Id = 6, QuestionId = 3, OptionText = "Khá đều đặn", Score = 2, Order = 2 },
                new QuestionOption { Id = 7, QuestionId = 3, OptionText = "Không đều", Score = 5, Order = 3 },
                new QuestionOption { Id = 8, QuestionId = 3, OptionText = "Rất không đều hoặc vô kinh", Score = 8, Order = 4 },

                // Question 4 options
                new QuestionOption { Id = 9, QuestionId = 4, OptionText = "Lạc nội mạc tử cung", Score = 6, Order = 1 },
                new QuestionOption { Id = 10, QuestionId = 4, OptionText = "Buồng trứng đa nang", Score = 5, Order = 2 },
                new QuestionOption { Id = 11, QuestionId = 4, OptionText = "U xơ tử cung", Score = 4, Order = 3 },
                new QuestionOption { Id = 12, QuestionId = 4, OptionText = "Viêm nhiễm phụ khoa", Score = 3, Order = 4 },
                new QuestionOption { Id = 13, QuestionId = 4, OptionText = "Không có", Score = 0, Order = 5 },

                // Question 5 options
                new QuestionOption { Id = 14, QuestionId = 5, OptionText = "Không hút", Score = 0, Order = 1 },
                new QuestionOption { Id = 15, QuestionId = 5, OptionText = "Thỉnh thoảng", Score = 2, Order = 2 },
                new QuestionOption { Id = 16, QuestionId = 5, OptionText = "Thường xuyên", Score = 5, Order = 3 },

                // Question 7 options
                new QuestionOption { Id = 17, QuestionId = 7, OptionText = "Không", Score = 0, Order = 1 },
                new QuestionOption { Id = 18, QuestionId = 7, OptionText = "Có", Score = 4, Order = 2 },

                // Question 8 options
                new QuestionOption { Id = 19, QuestionId = 8, OptionText = "2-3 lần/tuần", Score = 0, Order = 1 },
                new QuestionOption { Id = 20, QuestionId = 8, OptionText = "1 lần/tuần", Score = 2, Order = 2 },
                new QuestionOption { Id = 21, QuestionId = 8, OptionText = "Dưới 1 lần/tuần", Score = 4, Order = 3 }
            };

            modelBuilder.Entity<QuestionOption>().HasData(options);
        }
    }
} 