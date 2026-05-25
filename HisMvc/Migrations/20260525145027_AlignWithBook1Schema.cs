using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HisMvc.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithBook1Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_Staffs_DischargedByStaffStaffId",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaims_Staffs_ApprovedByStaffStaffId",
                table: "InsuranceClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Staffs_StaffId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOrders_Staffs_ExecutedByStaffStaffId",
                table: "MedicalOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_VitalSigns_Staffs_RecordedByStaffStaffId",
                table: "VitalSigns");

            migrationBuilder.DropIndex(
                name: "IX_VitalSigns_RecordedByStaffStaffId",
                table: "VitalSigns");

            migrationBuilder.DropIndex(
                name: "IX_MedicalOrders_ExecutedByStaffStaffId",
                table: "MedicalOrders");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceClaims_ApprovedByStaffStaffId",
                table: "InsuranceClaims");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_DischargedByStaffStaffId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "RecordedByStaffStaffId",
                table: "VitalSigns");

            migrationBuilder.DropColumn(
                name: "ExecutedByStaffStaffId",
                table: "MedicalOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedByStaffStaffId",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "DischargedByStaffStaffId",
                table: "Admissions");

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PrescriptionItems",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuranceCoveragePercent",
                table: "Patients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PatientAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuranceAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "HasInsurance",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_RecordedBy",
                table: "VitalSigns",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalOrders_ExecutedBy",
                table: "MedicalOrders",
                column: "ExecutedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ApprovedBy",
                table: "InsuranceClaims",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_DischargedBy",
                table: "Admissions",
                column: "DischargedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_Staffs_DischargedBy",
                table: "Admissions",
                column: "DischargedBy",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaims_Staffs_ApprovedBy",
                table: "InsuranceClaims",
                column: "ApprovedBy",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Staffs_StaffId",
                table: "InventoryTransactions",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalOrders_Staffs_ExecutedBy",
                table: "MedicalOrders",
                column: "ExecutedBy",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VitalSigns_Staffs_RecordedBy",
                table: "VitalSigns",
                column: "RecordedBy",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_Staffs_DischargedBy",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaims_Staffs_ApprovedBy",
                table: "InsuranceClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Staffs_StaffId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOrders_Staffs_ExecutedBy",
                table: "MedicalOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_VitalSigns_Staffs_RecordedBy",
                table: "VitalSigns");

            migrationBuilder.DropIndex(
                name: "IX_VitalSigns_RecordedBy",
                table: "VitalSigns");

            migrationBuilder.DropIndex(
                name: "IX_MedicalOrders_ExecutedBy",
                table: "MedicalOrders");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceClaims_ApprovedBy",
                table: "InsuranceClaims");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_DischargedBy",
                table: "Admissions");

            migrationBuilder.AddColumn<int>(
                name: "RecordedByStaffStaffId",
                table: "VitalSigns",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PrescriptionItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuranceCoveragePercent",
                table: "Patients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ExecutedByStaffStaffId",
                table: "MedicalOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PatientAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuranceAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<bool>(
                name: "HasInsurance",
                table: "Invoices",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByStaffStaffId",
                table: "InsuranceClaims",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DischargedByStaffStaffId",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_RecordedByStaffStaffId",
                table: "VitalSigns",
                column: "RecordedByStaffStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalOrders_ExecutedByStaffStaffId",
                table: "MedicalOrders",
                column: "ExecutedByStaffStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ApprovedByStaffStaffId",
                table: "InsuranceClaims",
                column: "ApprovedByStaffStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_DischargedByStaffStaffId",
                table: "Admissions",
                column: "DischargedByStaffStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_Staffs_DischargedByStaffStaffId",
                table: "Admissions",
                column: "DischargedByStaffStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaims_Staffs_ApprovedByStaffStaffId",
                table: "InsuranceClaims",
                column: "ApprovedByStaffStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Staffs_StaffId",
                table: "InventoryTransactions",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalOrders_Staffs_ExecutedByStaffStaffId",
                table: "MedicalOrders",
                column: "ExecutedByStaffStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_VitalSigns_Staffs_RecordedByStaffStaffId",
                table: "VitalSigns",
                column: "RecordedByStaffStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId");
        }
    }
}
