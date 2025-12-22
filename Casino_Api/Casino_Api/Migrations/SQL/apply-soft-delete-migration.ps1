# PowerShell Script to Apply Soft Delete Migration
# Run this from the Casino_Api directory

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "AdminUsers Soft Delete Migration" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Database connection details
$server = "localhost"
$database = "casino_db"
$username = "casino_user"
$password = "Connect2MYSQL5er73R"

Write-Host "Database: $database" -ForegroundColor Yellow
Write-Host "Server: $server" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Do you want to apply the soft delete migration? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "Migration cancelled." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Applying migration..." -ForegroundColor Yellow

# SQL commands to run
$sql = @"
USE $database;

-- Add IsDeleted column
ALTER TABLE AdminUsers 
ADD COLUMN IsDeleted BOOLEAN NOT NULL DEFAULT 0;

-- Add DeletedAt column
ALTER TABLE AdminUsers 
ADD COLUMN DeletedAt DATETIME NULL;

-- Add index on IsDeleted
CREATE INDEX IX_AdminUsers_IsDeleted ON AdminUsers(IsDeleted);

-- Add composite index
CREATE INDEX IX_AdminUsers_IsDeleted_CreatedAt ON AdminUsers(IsDeleted, CreatedAt DESC);

-- Verify
SELECT 'Migration completed successfully!' as Status;
"@

try {
    # Try to execute using mysql command line tool
    $sql | mysql -h $server -u $username -p$password 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Migration applied successfully!" -ForegroundColor Green
        Write-Host ""
   Write-Host "Verifying columns..." -ForegroundColor Yellow
 
        # Verify the migration
        $verifySql = @"
USE $database;
DESCRIBE AdminUsers;
"@
      
    Write-Host ""
        Write-Host "AdminUsers Table Schema:" -ForegroundColor Cyan
        $verifySql | mysql -h $server -u $username -p$password
        
    } else {
        Write-Host "? Migration failed! Check MySQL connection." -ForegroundColor Red
    }
} catch {
    Write-Host "? Error executing migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run the migration manually in MySQL Workbench:" -ForegroundColor Yellow
    Write-Host "File: Casino_Api/Migrations/SQL/20250104_AddSoftDeleteToAdminUsers.sql" -ForegroundColor White
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Restart your application (Shift+F5, then F5)" -ForegroundColor White
Write-Host "2. Test the soft delete functionality" -ForegroundColor White
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Read-Host "Press Enter to exit"
