# Soft Delete Implementation for AdminUsers

## Overview
This implementation adds soft delete functionality to the `AdminUsers` table, allowing admin users to be "deleted" without permanently removing them from the database.

## Changes Made

### 1. Database Schema Changes
**File:** `Casino_Api/Migrations/SQL/20250104_AddSoftDeleteToAdminUsers.sql`

Two new columns added to `AdminUsers` table:
- `IsDeleted` (BOOLEAN, NOT NULL, DEFAULT 0) - Indicates if the record is soft-deleted
- `DeletedAt` (DATETIME, NULL) - Timestamp of when the record was soft-deleted

**Indexes added for performance:**
- `IX_AdminUsers_IsDeleted` - Simple index on IsDeleted column
- `IX_AdminUsers_IsDeleted_CreatedAt` - Composite index for filtered sorting

### 2. Model Updates
**File:** `Casino_Api/Models/AdminUser.cs`

```csharp
public bool IsDeleted { get; set; } = false;
public DateTime? DeletedAt { get; set; }
```

### 3. Repository Interface Updates
**File:** `Casino_Api/Repositories/Interfaces/IAdminUserRepository.cs`

New methods added:
- `GetAllIncludingDeletedAsync()` - Retrieves all records including soft-deleted
- `RestoreAsync(int id)` - Restores a soft-deleted record
- `HardDeleteAsync(int id)` - Permanently deletes a record

### 4. Repository Implementation Updates
**File:** `Casino_Api/Repositories/Implementations/AdminUserRepository.cs`

**Modified existing methods:**
- All SELECT queries now filter by `IsDeleted = 0` (only active records)
- `DeleteAsync()` now performs soft delete (sets IsDeleted = 1, DeletedAt = NOW())
- All SQL queries include `IsDeleted` and `DeletedAt` columns

**New methods implemented:**
- `GetAllIncludingDeletedAsync()` - No IsDeleted filter
- `RestoreAsync()` - Sets IsDeleted = 0, DeletedAt = NULL
- `HardDeleteAsync()` - Physical DELETE from database

### 5. Controller Updates
**File:** `Casino_Api/Controllers/AdminUsersController.cs`

**New endpoints:**
- `POST /api/adminusers/{id}/restore` - Restore a soft-deleted admin user
- `GET /api/adminusers/all-including-deleted` - Get all admins including deleted ones

## Migration Steps

### Step 1: Run the SQL Migration
Execute the migration script in MySQL:

```bash
# Option 1: Using MySQL Command Line
mysql -u casino_user -p casino_db < Casino_Api/Migrations/SQL/20250104_AddSoftDeleteToAdminUsers.sql

# Option 2: Using MySQL Workbench
# - Open the SQL file in MySQL Workbench
# - Execute the script
```

Or run directly in MySQL console:

```sql
USE casino_db;

ALTER TABLE AdminUsers 
ADD COLUMN IsDeleted BOOLEAN NOT NULL DEFAULT 0;

ALTER TABLE AdminUsers 
ADD COLUMN DeletedAt DATETIME NULL;

CREATE INDEX IX_AdminUsers_IsDeleted ON AdminUsers(IsDeleted);
CREATE INDEX IX_AdminUsers_IsDeleted_CreatedAt ON AdminUsers(IsDeleted, CreatedAt DESC);
```

### Step 2: Restart Your Application
Since this involves schema changes, you need to restart:
1. Stop the current debug session (Shift+F5)
2. Rebuild the solution (Ctrl+Shift+B)
3. Start debugging (F5)

## API Usage Examples

### 1. Delete Admin User (Soft Delete)
```http
DELETE https://localhost:<port>/api/adminusers/5?apiKey=your-api-key
```
**Result:** Sets `IsDeleted = 1`, `DeletedAt = NOW()` - Record still in database but hidden

### 2. Get All Active Admin Users (Default)
```http
GET https://localhost:<port>/api/adminusers?apiKey=your-api-key
```
**Result:** Returns only records where `IsDeleted = 0`

### 3. Get All Admin Users Including Deleted
```http
GET https://localhost:<port>/api/adminusers/all-including-deleted?apiKey=your-api-key
```
**Result:** Returns all records regardless of `IsDeleted` status

### 4. Restore a Soft-Deleted Admin User
```http
POST https://localhost:<port>/api/adminusers/5/restore?apiKey=your-api-key
```
**Result:** Sets `IsDeleted = 0`, `DeletedAt = NULL` - Record becomes active again

**Response (200 OK):**
```json
{
  "message": "Admin user restored successfully"
}
```

### 5. Hard Delete (Permanent - Not exposed by default)
To permanently delete, you'd need to add a controller endpoint for `HardDeleteAsync()`.
This is intentionally not exposed for safety.

## Benefits

### Data Safety
- ? No accidental permanent data loss
- ? Audit trail maintained (DeletedAt timestamp)
- ? Easy recovery of deleted records

### Performance
- ? Indexed queries for fast filtering
- ? Minimal overhead on active record queries

### Compliance
- ? Supports data retention policies
- ? Audit trail for deleted records
- ? Can restore records for compliance investigations

## Database Verification

After running the migration, verify the schema:

```sql
-- Check new columns exist
DESCRIBE AdminUsers;

-- Verify indexes
SHOW INDEX FROM AdminUsers WHERE Key_name LIKE 'IX_AdminUsers_%';

-- Check default values
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'casino_db' 
  AND TABLE_NAME = 'AdminUsers'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt');
```

## Rollback (If Needed)

If you need to revert the migration:

```sql
ALTER TABLE AdminUsers DROP COLUMN IsDeleted;
ALTER TABLE AdminUsers DROP COLUMN DeletedAt;
DROP INDEX IX_AdminUsers_IsDeleted ON AdminUsers;
DROP INDEX IX_AdminUsers_IsDeleted_CreatedAt ON AdminUsers;
```

## Future Enhancements

### Possible additions:
1. **DeletedBy tracking** - Add `DeletedByUserId` column
2. **Auto-purge** - Background job to hard delete after X days
3. **Restore audit** - Track who restored and when
4. **Bulk operations** - Restore/delete multiple records at once

## Testing Checklist

- [ ] Run SQL migration successfully
- [ ] Restart application
- [ ] Create a new admin user
- [ ] Soft delete the admin user (DELETE endpoint)
- [ ] Verify user doesn't appear in GET /api/adminusers
- [ ] Verify user appears in GET /api/adminusers/all-including-deleted
- [ ] Restore the admin user (POST /api/adminusers/{id}/restore)
- [ ] Verify user appears in GET /api/adminusers again
- [ ] Check database directly to see IsDeleted and DeletedAt values

---

**Implementation Date:** January 4, 2025  
**Version:** 1.0  
**Author:** GitHub Copilot
