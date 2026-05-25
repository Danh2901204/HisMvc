# Dong bo du lieu giua 2 instance SQL Server (chay 1 lan hoac khi can)
# - Source: LocalDB (localdb)\mssqllocaldb
# - Target: SQL Server localhost (mac dinh) — trung voi Azure Data Studio
#
# Cach chay (PowerShell):
#   cd c:\UTT\Thuctap\HisMvc\HisMvc\Scripts
#   .\Sync-HisMvcDatabases.ps1
#   .\Sync-HisMvcDatabases.ps1 -Direction Both   # hai chieu (Patients theo Phone)

param(
    [ValidateSet("LocalDbToLocalhost", "LocalhostToLocalDb", "Both")]
    [string]$Direction = "LocalDbToLocalhost",
    [string]$Database = "HIS_MVC_DB",
    [string]$LocalDbServer = "(localdb)\mssqllocaldb",
    [string]$MainServer = "localhost"
)

$ErrorActionPreference = "Stop"

function Invoke-SqlScalar($Server, $Query) {
    $result = sqlcmd -S $Server -d $Database -Q $Query -W -h -1 2>&1
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed on $Server : $result" }
    return ($result | Where-Object { $_ -and $_ -notmatch "rows affected" } | Select-Object -First 1).ToString().Trim()
}

function Get-Patients($Server) {
    $lines = sqlcmd -S $Server -d $Database -Q @"
SET NOCOUNT ON;
SELECT
    PatientId, FullName, Phone,
    CONVERT(varchar(10), Dob, 23) AS Dob,
    CAST(Gender AS int) AS Gender,
    ISNULL(IdentityNumber,'') AS IdentityNumber,
    ISNULL(Address,'') AS Address,
    ISNULL(InsuranceNumber,'') AS InsuranceNumber,
    ISNULL(InsuranceType,'') AS InsuranceType,
    CONVERT(varchar(30), InsuranceExpiry, 126) AS InsuranceExpiry,
    CAST(InsuranceCoveragePercent AS varchar(20)) AS InsuranceCoveragePercent,
    ISNULL(InsuranceHospital,'') AS InsuranceHospital
FROM Patients;
"@ -W -s "`t" -h -1

    $rows = @()
    foreach ($line in $lines) {
        if (-not $line -or $line -match "^\(\d+ rows") { continue }
        $p = $line -split "`t"
        if ($p.Count -lt 12) { continue }
        $rows += [PSCustomObject]@{
            PatientId = [int]$p[0]
            FullName = $p[1]
            Phone = $p[2].Trim()
            Dob = if ($p[3]) { $p[3] } else { $null }
            Gender = [int]$p[4]
            IdentityNumber = $p[5]
            Address = $p[6]
            InsuranceNumber = $p[7]
            InsuranceType = $p[8]
            InsuranceExpiry = $p[9]
            InsuranceCoveragePercent = [decimal]$p[10]
            InsuranceHospital = $p[11]
        }
    }
    return $rows
}

function Escape-Sql($s) {
    if ([string]::IsNullOrEmpty($s)) { return "NULL" }
    return "N'" + ($s -replace "'", "''") + "'"
}

function Copy-MissingPatients($SourceServer, $TargetServer) {
    $source = Get-Patients $SourceServer
    $targetPhones = @{}
    (Get-Patients $TargetServer) | ForEach-Object { $targetPhones[$_.Phone] = $true }

    $inserted = 0
    foreach ($p in $source) {
        if ($targetPhones.ContainsKey($p.Phone)) { continue }

        $dob = if ($p.Dob -and $p.Dob -match '^\d{4}-\d{2}-\d{2}$') { "'$($p.Dob)'" } else { "NULL" }
        $expiry = if ($p.InsuranceExpiry -and $p.InsuranceExpiry -match '^\d{4}-\d{2}-\d{2}') { "'$($p.InsuranceExpiry.Substring(0,10))'" } else { "NULL" }
        $idNum = if ($p.IdentityNumber) { Escape-Sql $p.IdentityNumber } else { "NULL" }
        $addr = if ($p.Address) { Escape-Sql $p.Address } else { "NULL" }
        $insNum = if ($p.InsuranceNumber) { Escape-Sql $p.InsuranceNumber } else { "NULL" }
        $insType = if ($p.InsuranceType) { Escape-Sql $p.InsuranceType } else { "NULL" }
        $insHosp = if ($p.InsuranceHospital) { Escape-Sql $p.InsuranceHospital } else { "NULL" }

        $coverage = if ($null -ne $p.InsuranceCoveragePercent) { $p.InsuranceCoveragePercent } else { 0 }
        $sql = "INSERT INTO Patients (FullName, Phone, Dob, Gender, IdentityNumber, Address, InsuranceNumber, InsuranceType, InsuranceExpiry, InsuranceCoveragePercent, InsuranceHospital) VALUES ($(Escape-Sql $p.FullName), $(Escape-Sql $p.Phone), $dob, $($p.Gender), $idNum, $addr, $insNum, $insType, $expiry, $coverage, $insHosp);"

        $out = sqlcmd -S $TargetServer -d $Database -Q $sql -b 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "  ! Bo qua $($p.Phone): $out"
            continue
        }
        $inserted++
        Write-Host "  + $($p.Phone) - $($p.FullName)"
    }
    return $inserted
}

Write-Host "=== Dong bo HIS_MVC_DB ===" -ForegroundColor Cyan
Write-Host "LocalDB : $LocalDbServer"
Write-Host "Main    : $MainServer"
Write-Host ""

if ($Direction -eq "LocalDbToLocalhost" -or $Direction -eq "Both") {
    Write-Host "[1] LocalDB -> localhost (benh nhan thieu)" -ForegroundColor Yellow
    $n = Copy-MissingPatients $LocalDbServer $MainServer
    Write-Host "  Da them $n benh nhan vao localhost." -ForegroundColor Green
}

if ($Direction -eq "LocalhostToLocalDb" -or $Direction -eq "Both") {
    Write-Host "[2] localhost -> LocalDB (benh nhan thieu)" -ForegroundColor Yellow
    $n = Copy-MissingPatients $MainServer $LocalDbServer
    Write-Host "  Da them $n benh nhan vao LocalDB." -ForegroundColor Green
}

Write-Host ""
Write-Host "So luong benh nhan sau dong bo:" -ForegroundColor Cyan
$lc = Invoke-SqlScalar $LocalDbServer "SELECT COUNT(*) FROM Patients"
$mc = Invoke-SqlScalar $MainServer "SELECT COUNT(*) FROM Patients"
Write-Host "  LocalDB   : $lc"
Write-Host "  localhost : $mc"
Write-Host ""
Write-Host "Luu y: Chi dong bo bang Patients (theo So dien thoai)." -ForegroundColor DarkYellow
Write-Host "Lich hen, luot kham, hoa don van co the khac nhau - nen chi dung 1 DB cho app (localhost)." -ForegroundColor DarkYellow
