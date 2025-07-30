using Microsoft.EntityFrameworkCore.Migrations;

namespace InfertilityApp.Migrations
{
    public partial class AddMedicalRecordIdToTreatment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MedicalRecordId",
                table: "Treatments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_MedicalRecordId",
                table: "Treatments",
                column: "MedicalRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Treatments_MedicalRecords_MedicalRecordId",
                table: "Treatments",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Treatments_MedicalRecords_MedicalRecordId",
                table: "Treatments");

            migrationBuilder.DropIndex(
                name: "IX_Treatments_MedicalRecordId",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "MedicalRecordId",
                table: "Treatments");
        }
    }
}
