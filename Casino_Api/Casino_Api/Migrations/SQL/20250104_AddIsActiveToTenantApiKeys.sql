-- =============================================
-- Migration: Add IsActive to TenantApiKeys (SAFE VERSION)
-- Date: 2025-01-04
-- Description: Adds IsActive column to TenantApiKeys table for key management
-- This version checks if column exists before adding
-- =============================================

USE casino_db;

-- Check if IsActive column exists
SET @column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'casino_db'
      AND TABLE_NAME = 'TenantApiKeys'
      AND COLUMN_NAME = 'IsActive'
);

-- Add IsActive column only if it doesn't exist
SET @sql_add_column = IF(@column_exists = 0,
    'ALTER TABLE TenantApiKeys ADD COLUMN IsActive BOOLEAN NOT NULL DEFAULT 1',
 'SELECT "IsActive column already exists" as Message'
);

PREPARE stmt FROM @sql_add_column;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index on IsActive (will skip if already exists)
CREATE INDEX IF NOT EXISTS IX_TenantApiKeys_IsActive ON TenantApiKeys(IsActive);

-- Add composite index for API key validation queries (will skip if already exists)
CREATE INDEX IF NOT EXISTS IX_TenantApiKeys_ApiKey_IsActive ON TenantApiKeys(ApiKey, IsActive);

-- Verify the migration
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'casino_db' 
  AND TABLE_NAME = 'TenantApiKeys'
ORDER BY ORDINAL_POSITION;

-- Display all API keys with their active status
SELECT 
    Id, 
    TenantName, 
    ApiKey, 
    IsActive,
    CreatedAt
FROM TenantApiKeys
ORDER BY CreatedAt DESC;

-- Summary
SELECT 
    COUNT(*) as Total_Keys,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Active_Keys,
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) as Inactive_Keys
FROM TenantApiKeys;

-- =============================================
-- Rollback Script (if needed)
-- =============================================
-- ALTER TABLE TenantApiKeys DROP COLUMN IF EXISTS IsActive;
-- DROP INDEX IF EXISTS IX_TenantApiKeys_IsActive ON TenantApiKeys;
-- DROP INDEX IF EXISTS IX_TenantApiKeys_ApiKey_IsActive ON TenantApiKeys;
