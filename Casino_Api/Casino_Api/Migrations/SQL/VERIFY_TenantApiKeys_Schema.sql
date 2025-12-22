-- =============================================
-- Verification: Check TenantApiKeys Schema
-- Date: 2025-01-04
-- Description: Verifies current schema and fixes any issues
-- =============================================

USE casino_db;

-- Check current schema
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT,
    COLUMN_KEY
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'casino_db' 
  AND TABLE_NAME = 'TenantApiKeys'
ORDER BY ORDINAL_POSITION;

-- Check if IsActive column exists
SELECT 
    COUNT(*) as IsActive_Exists
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'casino_db' 
  AND TABLE_NAME = 'TenantApiKeys'
  AND COLUMN_NAME = 'IsActive';

-- Show all existing records
SELECT * FROM TenantApiKeys;

-- =============================================
-- Fix: If IsActive column already exists but has wrong type/default
-- =============================================

-- Uncomment and run if IsActive exists but needs to be fixed:
-- ALTER TABLE TenantApiKeys MODIFY COLUMN IsActive BOOLEAN NOT NULL DEFAULT 1;

-- =============================================
-- Fix: If IsActive column doesn't exist
-- =============================================

-- Uncomment and run if IsActive doesn't exist:
-- ALTER TABLE TenantApiKeys ADD COLUMN IsActive BOOLEAN NOT NULL DEFAULT 1;

-- =============================================
-- Fix: Add missing indexes (safe to run even if they exist)
-- =============================================

-- Drop existing indexes if they exist (MySQL won't error if they don't exist with IF EXISTS)
-- DROP INDEX IF EXISTS IX_TenantApiKeys_IsActive ON TenantApiKeys;
-- DROP INDEX IF EXISTS IX_TenantApiKeys_ApiKey_IsActive ON TenantApiKeys;

-- Create indexes
-- CREATE INDEX IX_TenantApiKeys_IsActive ON TenantApiKeys(IsActive);
-- CREATE INDEX IX_TenantApiKeys_ApiKey_IsActive ON TenantApiKeys(ApiKey, IsActive);

-- =============================================
-- Verify API Keys
-- =============================================

-- Ensure at least one active API key exists
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN 'WARNING: No API keys exist!'
 WHEN SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) = 0 THEN 'WARNING: No active API keys!'
        ELSE 'OK: Active API keys exist'
    END as API_Key_Status,
    COUNT(*) as Total_Keys,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Active_Keys
FROM TenantApiKeys;
