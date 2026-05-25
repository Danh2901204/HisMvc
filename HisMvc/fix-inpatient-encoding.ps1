# ==================================================
# FAST FIX VIETNAMESE ENCODING IN INPATIENT MODULE
# ==================================================

Write-Host "Fixing Vietnamese encoding in Inpatient module..." -ForegroundColor Cyan

# Simple character replacements (most common)
$replacements = @{
    # Common words
    'Thêm' = 'Them'; 'thêm' = 'them'
    'S?a' = 'Sua'; 's?a' = 'sua'  
    'Xóa' = 'Xoa'; 'xóa' = 'xoa'
    'Tìm' = 'Tim'; 'tìm' = 'tim'
    'C?p' = 'Cap'; 'c?p' = 'cap'
    'Nh?t' = 'Nhat'; 'nh?t' = 'nhat'
    '?ã' = 'Da'; '?ã' = 'da'
    'Thành' = 'Thanh'; 'thành' = 'thanh'
    'Công' = 'Cong'; 'công' = 'cong'
    'L?i' = 'Loi'; 'l?i' = 'loi'
    'Không' = 'Khong'; 'không' = 'khong'
    'Nh?p' = 'Nhap'; 'nh?p' = 'nhap'
    'Xu?t' = 'Xuat'; 'xu?t' = 'xuat'
    'Vi?n' = 'Vien'; 'vi?n' = 'vien'
    'B?nh' = 'Benh'; 'b?nh' = 'benh'
    'Nhân' = 'Nhan'; 'nhân' = 'nhan'
    'Gi??ng' = 'Giuong'; 'gi??ng' = 'giuong'
    'Bu?ng' = 'Buong'; 'bu?ng' = 'buong'
    'Tr?ng' = 'Trong'; 'tr?ng' = 'trong'
    '?ang' = 'Dang'; '?ang' = 'dang'
    'S?' = 'Su'; 's?' = 'su'
    'D?ng' = 'Dung'; 'd?ng' = 'dung'
    'S?' = 'So'; 's?' = 'so'
    'H?' = 'Ho'; 'h?' = 'ho'
    'S?' = 'So'; 's?' = 'so'
    'Tên' = 'Ten'; 'tên' = 'ten'
    'Mã' = 'Ma'; 'mã' = 'ma'
    '??a' = 'Dia'; '??a' = 'dia'
    'Ch?' = 'Chi'; 'ch?' = 'chi'
    'Ghi' = 'Ghi'; 'ghi' = 'ghi'
    'Chú' = 'Chu'; 'chú' = 'chu'
    'Ký' = 'Ky'; 'ký' = 'ky'
    '?i?n' = 'Dien'; '?i?n' = 'dien'
    'Tho?i' = 'Thoai'; 'tho?i' = 'thoai'
    'Khám' = 'Kham'; 'khám' = 'kham'
    'Ch?a' = 'Chua'; 'ch?a' = 'chua'
    'Tr?' = 'Tri'; 'tr?' = 'tri'
    '?i?u' = 'Dieu'; '?i?u' = 'dieu'
    'Bác' = 'Bac'; 'bác' = 'bac'
    'S?' = 'Si'; 's?' = 'si'
    'Y' = 'Y'; 'y' = 'y'
    'Tá' = 'Ta'; 'tá' = 'ta'
    'D??c' = 'Duoc'; 'd??c' = 'duoc'
    'Thu?c' = 'Thuoc'; 'thu?c' = 'thuoc'
    '??n' = 'Don'; '??n' = 'don'
    'C?p' = 'Cap'; 'c?p' = 'cap'
    'Phát' = 'Phat'; 'phát' = 'phat'
    'Hóa' = 'Hoa'; 'hóa' = 'hoa'
    '??n' = 'Don'; '??n' = 'don'
    'Ti?n' = 'Tien'; 'ti?n' = 'tien'
    'Thanh' = 'Thanh'; 'thanh' = 'thanh'
    'Toán' = 'Toan'; 'toán' = 'toan'
    'Qu?n' = 'Quan'; 'qu?n' = 'quan'
    'Lý' = 'Ly'; 'lý' = 'ly'
    'Thông' = 'Thong'; 'thông' = 'thong'
    'Tin' = 'Tin'; 'tin' = 'tin'
    'Ng??i' = 'Nguoi'; 'ng??i' = 'nguoi'
    'Dùng' = 'Dung'; 'dùng' = 'dung'
    'H?' = 'He'; 'h?' = 'he'
    'Th?ng' = 'Thong'; 'th?ng' = 'thong'
    'Phòng' = 'Phong'; 'phòng' = 'phong'
    'Khoa' = 'Khoa'; 'khoa' = 'khoa'
    'K?' = 'Ky'; 'k?' = 'ky'
    'Thu?t' = 'Thuat'; 'thu?t' = 'thuat'
    'Ch?c' = 'Chuc'; 'ch?c' = 'chuc'
    'N?ng' = 'Nang'; 'n?ng' = 'nang'
    'M?t' = 'Mat'; 'm?t' = 'mat'
    'Kh?u' = 'Khau'; 'kh?u' = 'khau'
    '??ng' = 'Dang'; '??ng' = 'dang'
    'Nh?p' = 'Nhap'; 'nh?p' = 'nhap'
    'K?t' = 'Ket'; 'k?t' = 'ket'
    'Qu?' = 'Qua'; 'qu?' = 'qua'
    'Xét' = 'Xet'; 'xét' = 'xet'
    'Nghi?m' = 'Nghiem'; 'nghi?m' = 'nghiem'
    'Hình' = 'Hinh'; 'hình' = 'hinh'
    '?nh' = 'Anh'; '?nh' = 'anh'
    'Ch?n' = 'Chan'; 'ch?n' = 'chan'
    '?oán' = 'Doan'; '?oán' = 'doan'
    '??nh' = 'Dinh'; '??nh' = 'dinh'
    'D?ch' = 'Dich'; 'd?ch' = 'dich'
    'V?' = 'Vu'; 'v?' = 'vu'
    'L?ch' = 'Lich'; 'l?ch' = 'lich'
    'H?n' = 'Hen'; 'h?n' = 'hen'
    'Ngày' = 'Ngay'; 'ngày' = 'ngay'
    'Gi?' = 'Gio'; 'gi?' = 'gio'
    'Phút' = 'Phut'; 'phút' = 'phut'
    '??i' = 'Doi'; '??i' = 'doi'
    'Ch?' = 'Cho'; 'ch?' = 'cho'
}

function Fix-File {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) {
        return
    }
    
    try {
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        $originalContent = $content
        
        foreach ($key in $replacements.Keys) {
            $content = $content.Replace($key, $replacements[$key])
        }
        
        if ($originalContent -ne $content) {
            [System.IO.File]::WriteAllText($FilePath, $content, [System.Text.Encoding]::UTF8)
            Write-Host "  [FIXED] $FilePath" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "  [ERROR] $FilePath : $($_.Exception.Message)" -ForegroundColor Red
    }
    
    return $false
}

$fixed = 0

# Fix Controllers
Write-Host "`nFixing Controllers..." -ForegroundColor Yellow
if (Fix-File "HisMvc\Areas\Inpatient\Controllers\HomeController.cs") { $fixed++ }
if (Fix-File "HisMvc\Areas\Inpatient\Controllers\WardController.cs") { $fixed++ }

# Fix Views
Write-Host "`nFixing Views..." -ForegroundColor Yellow
$viewFiles = @(
    "HisMvc\Areas\Inpatient\Views\Home\Index.cshtml",
    "HisMvc\Areas\Inpatient\Views\Home\Detail.cshtml",
    "HisMvc\Areas\Inpatient\Views\Home\Admit.cshtml",
    "HisMvc\Areas\Inpatient\Views\Home\Discharge.cshtml",
    "HisMvc\Areas\Inpatient\Views\Home\AddVitalSign.cshtml",
    "HisMvc\Areas\Inpatient\Views\Ward\Index.cshtml",
    "HisMvc\Areas\Inpatient\Views\Ward\BedMap.cshtml",
    "HisMvc\Areas\Inpatient\Views\Ward\Create.cshtml",
    "HisMvc\Areas\Inpatient\Views\Ward\Edit.cshtml",
    "HisMvc\Areas\Inpatient\Views\Ward\AddBed.cshtml"
)

foreach ($file in $viewFiles) {
    if (Fix-File $file) { $fixed++ }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "DONE! Fixed $fixed files" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nPlease rebuild the project:" -ForegroundColor Yellow
Write-Host "  dotnet build" -ForegroundColor White
