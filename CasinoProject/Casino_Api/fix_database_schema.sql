-- Complete Database Recreation Script for Casino API
-- This will drop the old database and let EF Core recreate it with all correct columns

-- Drop the existing database (this will delete all data!)
DROP DATABASE IF EXISTS casino_db;

-- Create a fresh database
CREATE DATABASE casino_db;

-- Use the database
USE casino_db;

-- That's it! Now restart your API with: dotnet run
-- The API will automatically create all tables with the correct schema using EnsureCreated()

-- After the API starts, verify the tables were created:
SHOW TABLES;

-- Check Users table structure:
DESCRIBE Users;

-- Check AdminUsers table structure:
DESCRIBE AdminUsers;

-- Check TenantApiKeys table structure:
DESCRIBE TenantApiKeys;

-- View seeded data:
SELECT * FROM Users WHERE Username = 'admin';
SELECT * FROM TenantApiKeys;
