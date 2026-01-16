# Test HIS MVC API Script
# Ch?y script này ?? test t?t c? API endpoints

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  HIS MVC API TEST SCRIPT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:7239/api/AppointmentsApi"
$today = (Get-Date).ToString("yyyy-MM-dd")

Write-Host "?? Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Function to test endpoint
function Test-Endpoint {
    param (
        [string]$Name,
        [string]$Url
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Cyan
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    try {
        $response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
        
        if ($response.success -eq $true) {
            Write-Host "? SUCCESS" -ForegroundColor Green
            
            # Display data count
            if ($response.departments) {
                Write-Host "   ? Found $($response.departments.Count) departments" -ForegroundColor Green
            }
            if ($response.doctors) {
                Write-Host "   ? Found $($response.doctors.Count) doctors" -ForegroundColor Green
            }
            if ($response.slots) {
                Write-Host "   ? Found $($response.slots.Count) time slots" -ForegroundColor Green
            }
        } else {
            Write-Host "??  Response success=false" -ForegroundColor Yellow
            Write-Host "   Message: $($response.message)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "? FAILED" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        
        if ($_.Exception.Message -match "Unable to connect") {
            Write-Host "   ? HIS MVC ch?a ch?y ho?c sai port!" -ForegroundColor Red
        }
    }
    
    Write-Host ""
}

# Test all endpoints
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TESTING ENDPOINTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Test-Endpoint "1. Get Departments" "$baseUrl/Departments"
Test-Endpoint "2. Get All Doctors" "$baseUrl/Doctors"
Test-Endpoint "3. Get Doctors by Department (ID=1)" "$baseUrl/Doctors?departmentId=1"
Test-Endpoint "4. Get Available Slots" "$baseUrl/AvailableSlots?date=$today"

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if HIS is running
try {
    $healthCheck = Invoke-WebRequest -Uri "http://localhost:7239" -Method Get -TimeoutSec 2 -ErrorAction Stop
    Write-Host "? HIS MVC is running on port 7239" -ForegroundColor Green
}
catch {
    Write-Host "? HIS MVC is NOT running or wrong port!" -ForegroundColor Red
    Write-Host "   ? Run HIS MVC in Visual Studio first (F5)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  NEXT STEPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. If all tests passed:" -ForegroundColor Yellow
Write-Host "   - Update web .env: USE_MOCK=false" -ForegroundColor White
Write-Host "   - Restart web: npm run dev" -ForegroundColor White
Write-Host ""
Write-Host "2. If tests failed:" -ForegroundColor Yellow
Write-Host "   - Make sure HIS MVC is running" -ForegroundColor White
Write-Host "   - Check port in Visual Studio Output" -ForegroundColor White
Write-Host "   - See TEST_API_GUIDE.md for troubleshooting" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to exit"
