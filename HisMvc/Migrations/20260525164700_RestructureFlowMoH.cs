using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HisMvc.Migrations
{
    /// <inheritdoc />
    public partial class RestructureFlowMoH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allergies_Patients_PatientId",
                table: "Allergies");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Patients_PatientId",
                table: "Encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Staffs_DoctorId",
                table: "Encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Encounters_EncounterId",
                table: "Orders");

            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MedicineBatches_MedicineId' AND object_id = OBJECT_ID('MedicineBatches')) DROP INDEX IX_MedicineBatches_MedicineId ON MedicineBatches;");

            migrationBuilder.AddColumn<int>(
                name: "Shift",
                table: "VitalSigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Staffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Staffs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffCode",
                table: "Staffs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Staffs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BhytCode",
                table: "Services",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BhytPrice",
                table: "Services",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "Services",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInBhytList",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "EncounterId",
                table: "Prescriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AdmissionId",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Ethnicity",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceHospitalCode",
                table: "Patients",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceValidFrom",
                table: "Patients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientCode",
                table: "Patients",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrderedByStaffId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "OrderResults",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultedByStaffId",
                table: "OrderResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BhytCode",
                table: "Medicines",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BhytPrice",
                table: "Medicines",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInBhytList",
                table: "Medicines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "EncounterId",
                table: "Invoices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AdmissionId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BedAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExamFeeAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicineAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaidByStaffId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Invoices",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServicesAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Bao dam co it nhat 1 Department trong DB (de gan default cho Encounter cu)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Departments)
                BEGIN
                    SET IDENTITY_INSERT Departments ON;
                    INSERT INTO Departments (DepartmentId, Code, Name) VALUES (1, 'DEFAULT', N'Khoa kham benh');
                    SET IDENTITY_INSERT Departments OFF;
                END");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Encounters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Suy ra DepartmentId tu Doctor.DepartmentId, neu khong co thi gan Department dau tien
            migrationBuilder.Sql(@"
                DECLARE @fallback INT = (SELECT TOP 1 DepartmentId FROM Departments ORDER BY DepartmentId);
                UPDATE e SET e.DepartmentId = ISNULL(s.DepartmentId, @fallback)
                FROM Encounters e LEFT JOIN Staffs s ON e.DoctorId = s.StaffId;
                UPDATE Encounters SET DepartmentId = @fallback
                WHERE DepartmentId NOT IN (SELECT DepartmentId FROM Departments);");

            migrationBuilder.AddColumn<string>(
                name: "EncounterCode",
                table: "Encounters",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FollowUpDate",
                table: "Encounters",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icd10Primary",
                table: "Encounters",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icd10PrimaryName",
                table: "Encounters",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icd10Secondary",
                table: "Encounters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Encounters",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueNumber",
                table: "Encounters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QueuedAt",
                table: "Encounters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "Encounters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Encounters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AllergyType",
                table: "Allergies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DischargeResult",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icd10Admission",
                table: "Admissions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icd10Discharge",
                table: "Admissions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icd10DischargeSecondary",
                table: "Admissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_StaffCode",
                table: "Staffs",
                column: "StaffCode",
                unique: true,
                filter: "[StaffCode] IS NOT NULL AND [StaffCode] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Code",
                table: "Services",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL AND [Code] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_AdmissionId",
                table: "Prescriptions",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_IdentityNumber",
                table: "Patients",
                column: "IdentityNumber",
                unique: true,
                filter: "[IdentityNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientCode",
                table: "Patients",
                column: "PatientCode",
                unique: true,
                filter: "[PatientCode] IS NOT NULL AND [PatientCode] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true,
                filter: "[OrderCode] IS NOT NULL AND [OrderCode] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderedByStaffId",
                table: "Orders",
                column: "OrderedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderResults_ResultedByStaffId",
                table: "OrderResults",
                column: "ResultedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_MedicineId_BatchNumber",
                table: "MedicineBatches",
                columns: new[] { "MedicineId", "BatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AdmissionId",
                table: "Invoices",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaidByStaffId",
                table: "Invoices",
                column: "PaidByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceConfigs_InsuranceType",
                table: "InsuranceConfigs",
                column: "InsuranceType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_DepartmentId",
                table: "Encounters",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_EncounterCode",
                table: "Encounters",
                column: "EncounterCode",
                unique: true,
                filter: "[EncounterCode] IS NOT NULL AND [EncounterCode] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_Status",
                table: "Encounters",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Allergies_Patients_PatientId",
                table: "Allergies",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            // Backfill DepartmentId truoc khi tao FK (rows cu khong co Department)
            migrationBuilder.Sql(@"
                DECLARE @fb INT = (SELECT TOP 1 DepartmentId FROM Departments ORDER BY DepartmentId);
                UPDATE e SET e.DepartmentId = ISNULL(s.DepartmentId, @fb)
                FROM Encounters e LEFT JOIN Staffs s ON e.DoctorId = s.StaffId
                WHERE e.DepartmentId = 0 OR e.DepartmentId NOT IN (SELECT DepartmentId FROM Departments);");

            // Tao FK voi NOCHECK de bo qua kiem tra du lieu cu (du lieu da duoc backfill)
            migrationBuilder.Sql(@"
                ALTER TABLE [Encounters] WITH NOCHECK
                ADD CONSTRAINT [FK_Encounters_Departments_DepartmentId]
                FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]);");

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_Patients_PatientId",
                table: "Encounters",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_Staffs_DoctorId",
                table: "Encounters",
                column: "DoctorId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Admissions_AdmissionId",
                table: "Invoices",
                column: "AdmissionId",
                principalTable: "Admissions",
                principalColumn: "AdmissionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Staffs_PaidByStaffId",
                table: "Invoices",
                column: "PaidByStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderResults_Staffs_ResultedByStaffId",
                table: "OrderResults",
                column: "ResultedByStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Encounters_EncounterId",
                table: "Orders",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "EncounterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Staffs_OrderedByStaffId",
                table: "Orders",
                column: "OrderedByStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Admissions_AdmissionId",
                table: "Prescriptions",
                column: "AdmissionId",
                principalTable: "Admissions",
                principalColumn: "AdmissionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allergies_Patients_PatientId",
                table: "Allergies");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Departments_DepartmentId",
                table: "Encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Patients_PatientId",
                table: "Encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Staffs_DoctorId",
                table: "Encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Admissions_AdmissionId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Staffs_PaidByStaffId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderResults_Staffs_ResultedByStaffId",
                table: "OrderResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Encounters_EncounterId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Staffs_OrderedByStaffId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Admissions_AdmissionId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_StaffCode",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Services_Code",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_AdmissionId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Patients_IdentityNumber",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PatientCode",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderedByStaffId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderResults_ResultedByStaffId",
                table: "OrderResults");

            migrationBuilder.DropIndex(
                name: "IX_MedicineBatches_MedicineId_BatchNumber",
                table: "MedicineBatches");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AdmissionId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_PaidByStaffId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceConfigs_InsuranceType",
                table: "InsuranceConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_DepartmentId",
                table: "Encounters");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_EncounterCode",
                table: "Encounters");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_Status",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "Shift",
                table: "VitalSigns");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "StaffCode",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "BhytCode",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "BhytPrice",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsInBhytList",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AdmissionId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Ethnicity",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceHospitalCode",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceValidFrom",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PatientCode",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderedByStaffId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "OrderResults");

            migrationBuilder.DropColumn(
                name: "ResultedByStaffId",
                table: "OrderResults");

            migrationBuilder.DropColumn(
                name: "BhytCode",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "BhytPrice",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "IsInBhytList",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "AdmissionId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BedAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExamFeeAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "MedicineAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaidByStaffId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ServicesAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "EncounterCode",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "Icd10Primary",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "Icd10PrimaryName",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "Icd10Secondary",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "QueueNumber",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "QueuedAt",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AllergyType",
                table: "Allergies");

            migrationBuilder.DropColumn(
                name: "DischargeResult",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "Icd10Admission",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "Icd10Discharge",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "Icd10DischargeSecondary",
                table: "Admissions");

            migrationBuilder.AlterColumn<int>(
                name: "EncounterId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EncounterId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_MedicineId",
                table: "MedicineBatches",
                column: "MedicineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Allergies_Patients_PatientId",
                table: "Allergies",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_Patients_PatientId",
                table: "Encounters",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_Staffs_DoctorId",
                table: "Encounters",
                column: "DoctorId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Encounters_EncounterId",
                table: "Orders",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "EncounterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
