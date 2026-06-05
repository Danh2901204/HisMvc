using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HisMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentKindForPublicBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.Sql(@"
-- Khoa lâm sàng: cho phép đặt lịch khám ngoại trú công khai
UPDATE Departments SET Kind = 1
WHERE Code IN ('KB', 'NOI', 'TMH', 'NGOAI', 'SAN', 'NHI', 'TIM', 'DEFAULT')
   OR Name LIKE N'%Khám bệnh%' OR Name LIKE N'%Khám benh%'
   OR Name LIKE N'%Tai Mũi%' OR Name LIKE N'%Tai Mui%'
   OR Name LIKE N'%Nội tổng hợp%' OR Name LIKE N'%Noi tong hop%'
   OR Name LIKE N'%Ngoại%' OR Name LIKE N'%Sản%' OR Name LIKE N'%Nhi%'
   OR Name LIKE N'%Tim mạch%' OR Name LIKE N'%Tim mach%';

-- Phòng hành chính, quản trị (không đặt lịch)
UPDATE Departments SET Kind = 2
WHERE Name LIKE N'%Công nghệ thông tin%' OR Name LIKE N'%Cong nghe thong tin%'
   OR Name LIKE N'%Hành chính%' OR Name LIKE N'%Hanh chinh%'
   OR Name LIKE N'%Tài chính%' OR Name LIKE N'%Tai chinh%'
   OR Name LIKE N'%Kế toán%' OR Name LIKE N'%Ke toan%'
   OR Name LIKE N'%Nhân sự%' OR Name LIKE N'%Nhan su%'
   OR Name LIKE N'%Vật tư%' OR Name LIKE N'%Quản trị%';

-- Khoa cận lâm sàng (chỉ đến theo chỉ định BS)
UPDATE Departments SET Kind = 3
WHERE Name LIKE N'%Xét nghiệm%' OR Name LIKE N'%Xet nghiem%'
   OR Name LIKE N'%Chẩn đoán hình%' OR Name LIKE N'%CDHA%'
   OR Code IN ('XN', 'CDHA', 'LAB');

-- Nội trú / hồi sức (không đặt lịch ngoại trú công khai)
UPDATE Departments SET Kind = 4
WHERE Name LIKE N'%hồi sức%' OR Name LIKE N'%Hoi suc%'
   OR Name LIKE N'%ICU%' OR Name LIKE N'%Cấp cứu nội trú%'
   OR Code LIKE '%ICU%';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Departments");
        }
    }
}
