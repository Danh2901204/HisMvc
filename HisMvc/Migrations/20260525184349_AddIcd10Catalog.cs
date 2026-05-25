using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HisMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddIcd10Catalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Icd10Catalogs",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Chapter = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsCommon = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icd10Catalogs", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Icd10Catalogs_Name",
                table: "Icd10Catalogs",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Icd10Catalogs");
        }
    }
}
