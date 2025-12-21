# Automated Database Recreation Script for Casino API
# Run this to fix database schema issues

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Casino API - Database Recreation Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# MySQL connection details
$mysqlUser = "root"
$mysqlPassword = "Connect2MYSQL5er73R"
$database = "casino_db"

Write-Host "??  WARNING: This will DELETE all data in $database!" -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "Continue? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "Cancelled." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Step 1: Dropping existing database..." -ForegroundColor Yellow

# Drop and recreate database
$sqlCommand = @"
DROP DATABASE IF EXISTS $database;
CREATE DATABASE $database;
"@

# Try to run MySQL command
try {
    $sqlCommand | mysql -u $mysqlUser -p$mysqlPassword 2>&1 | Out-Null
    Write-Host "? Database recreated successfully!" -ForegroundColor Green
} catch {
    Write-Host "? MySQL command failed. Trying alternative method..." -ForegroundColor Red
    Write-Host "Please run this SQL manually in MySQL Workbench:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host $sqlCommand -ForegroundColor White
    Write-Host ""
    Read-Host "Press Enter after running the SQL"
}

Write-Host ""
Write-Host "Step 2: Starting the API..." -ForegroundColor Yellow
Write-Host ""

# Navigate to API directory
$apiPath = "C:\Users\ahmad\source\repos\CasinoBackend\CasinoProject\Casino_Api"
Set-Location $apiPath

# Clean and build
Write-Host "Cleaning..." -ForegroundColor Gray
dotnet clean --verbosity quiet

Write-Host "Building..." -ForegroundColor Gray
dotnet build --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed!" -ForegroundColor Red
    exit
}

Write-Host "? Build successful!" -ForegroundColor Green
Write-Host ""
Write-Host "Step 3: Running the API..." -ForegroundColor Yellow
Write-Host "The API will create all tables with correct schema automatically." -ForegroundColor Cyan
Write-Host ""
Write-Host "After you see 'Now listening on:', test with:" -ForegroundColor Cyan
Write-Host ""
Write-Host "POST http://localhost:5001/api/auth/token" -ForegroundColor White
Write-Host '{' -ForegroundColor White
Write-Host '  "grant_type": "password",' -ForegroundColor White
Write-Host '  "username": "admin",' -ForegroundColor White
Write-Host '  "password": "admin123",' -ForegroundColor White
Write-Host '  "webapi_key": "default_tenant_api_key_12345"' -ForegroundColor White
Write-Host '}' -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the API." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

# Run the API
dotnet run
