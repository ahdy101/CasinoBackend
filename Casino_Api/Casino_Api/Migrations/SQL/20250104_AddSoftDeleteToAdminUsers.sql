-- =============================================
-- Migration: Add Soft Delete to AdminUsers
-- Date: 2025-01-04
-- Description: Adds IsDeleted and DeletedAt columns for soft delete functionality
-- =============================================

-- Add IsDeleted column (default to 0/false)
ALTER TABLE AdminUsers 
ADD COLUMN IsDeleted BOOLEAN NOT NULL DEFAULT 0;

-- Add DeletedAt column (nullable, tracks when record was soft deleted)
ALTER TABLE AdminUsers 
ADD COLUMN DeletedAt DATETIME NULL;

-- Add index on IsDeleted for better query performance
CREATE INDEX IX_AdminUsers_IsDeleted ON AdminUsers(IsDeleted);

-- Optional: Add a composite index for common queries
CREATE INDEX IX_AdminUsers_IsDeleted_CreatedAt ON AdminUsers(IsDeleted, CreatedAt DESC);

-- Verify the migration
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'casino_db' 
  AND TABLE_NAME = 'AdminUsers'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt');

-- =============================================
-- Rollback Script (if needed)
-- =============================================
-- ALTER TABLE AdminUsers DROP COLUMN IsDeleted;
-- ALTER TABLE AdminUsers DROP COLUMN DeletedAt;
-- DROP INDEX IX_AdminUsers_IsDeleted ON AdminUsers;
-- DROP INDEX IX_AdminUsers_IsDeleted_CreatedAt ON AdminUsers;
