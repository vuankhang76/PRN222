using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfertilityApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTreatmentWithAppointmentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Patients");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "Treatments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedicationSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsTaken = table.Column<bool>(type: "bit", nullable: false),
                    ActualTakenTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsReminded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationSchedules_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: true),
                    TestType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Results = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReferenceRange = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TestingLab = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DoctorId = table.Column<int>(type: "int", nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResults_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestResults_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestResults_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentOutcomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreatmentId = table.Column<int>(type: "int", nullable: false),
                    OutcomeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OutcomeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPregnant = table.Column<bool>(type: "bit", nullable: false),
                    PregnancyTestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PregnancyTestResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpectedDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualBirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumberOfBabies = table.Column<int>(type: "int", nullable: true),
                    DeliveryType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BabyGender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthWeight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BabyHealth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Complications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FinalStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DoctorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentOutcomes_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TreatmentOutcomes_Treatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "Treatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_AppointmentId",
                table: "Treatments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationSchedules_MedicationId",
                table: "MedicationSchedules",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_DoctorId",
                table: "TestResults",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_PartnerId",
                table: "TestResults",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_PatientId",
                table: "TestResults",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentOutcomes_DoctorId",
                table: "TreatmentOutcomes",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentOutcomes_TreatmentId",
                table: "TreatmentOutcomes",
                column: "TreatmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Treatments_Appointments_AppointmentId",
                table: "Treatments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Treatments_Appointments_AppointmentId",
                table: "Treatments");

            migrationBuilder.DropTable(
                name: "MedicationSchedules");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "TreatmentOutcomes");

            migrationBuilder.DropIndex(
                name: "IX_Treatments_AppointmentId",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Treatments");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
