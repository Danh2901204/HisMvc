using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HisMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceCoveragePercent",
                table: "Patients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceExpiry",
                table: "Patients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceHospital",
                table: "Patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceNumber",
                table: "Patients",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceType",
                table: "Patients",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceCode",
                table: "Invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EInvoiceIssuedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasInsurance",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "InsuranceClaimId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PatientAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InsuranceClaims",
                columns: table => new
                {
                    InsuranceClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EncounterId = table.Column<int>(type: "int", nullable: true),
                    AdmissionId = table.Column<int>(type: "int", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    InsuranceNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    InsuranceExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InsuranceType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CoveragePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InsuranceCovered = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PatientPayment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedByStaffStaffId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    XmlData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceClaims", x => x.InsuranceClaimId);
                    table.ForeignKey(
                        name: "FK_InsuranceClaims_Admissions_AdmissionId",
                        column: x => x.AdmissionId,
                        principalTable: "Admissions",
                        principalColumn: "AdmissionId");
                    table.ForeignKey(
                        name: "FK_InsuranceClaims_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "EncounterId");
                    table.ForeignKey(
                        name: "FK_InsuranceClaims_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InsuranceClaims_Staffs_ApprovedByStaffStaffId",
                        column: x => x.ApprovedByStaffStaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "InsuranceConfigs",
                columns: table => new
                {
                    InsuranceConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InsuranceType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultCoveragePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequireRegistration = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceConfigs", x => x.InsuranceConfigId);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceClaimItems",
                columns: table => new
                {
                    InsuranceClaimItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InsuranceClaimId = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ServiceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InsurancePaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PatientPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsInInsuranceList = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceClaimItems", x => x.InsuranceClaimItemId);
                    table.ForeignKey(
                        name: "FK_InsuranceClaimItems_InsuranceClaims_InsuranceClaimId",
                        column: x => x.InsuranceClaimId,
                        principalTable: "InsuranceClaims",
                        principalColumn: "InsuranceClaimId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InsuranceClaimId",
                table: "Invoices",
                column: "InsuranceClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaimItems_InsuranceClaimId",
                table: "InsuranceClaimItems",
                column: "InsuranceClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_AdmissionId",
                table: "InsuranceClaims",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ApprovedByStaffStaffId",
                table: "InsuranceClaims",
                column: "ApprovedByStaffStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ClaimCode",
                table: "InsuranceClaims",
                column: "ClaimCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_EncounterId",
                table: "InsuranceClaims",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_PatientId",
                table: "InsuranceClaims",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_InsuranceClaims_InsuranceClaimId",
                table: "Invoices",
                column: "InsuranceClaimId",
                principalTable: "InsuranceClaims",
                principalColumn: "InsuranceClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_InsuranceClaims_InsuranceClaimId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "InsuranceClaimItems");

            migrationBuilder.DropTable(
                name: "InsuranceConfigs");

            migrationBuilder.DropTable(
                name: "InsuranceClaims");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InsuranceClaimId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceCoveragePercent",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiry",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceHospital",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceType",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EInvoiceCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "EInvoiceIssuedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "HasInsurance",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InsuranceAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InsuranceClaimId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PatientAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "Invoices");
        }
    }
}
