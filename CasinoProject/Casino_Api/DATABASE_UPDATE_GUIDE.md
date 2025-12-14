# Database Update Guide

## What Was Changed

### 1. AdminUser Model
Added `Email` property to match the User model:
- Email column (VARCHAR(100), Required, Unique)
- Default admin email: admin@casinoapi.com

### 2. Database Tables

#### Users Table (Already exists with):
- Id
- Username (unique)
- **Email (unique)** ?
- **PasswordHash** ?
- Balance
- CreatedAt
- TenantId

#### AdminUsers Table (Updated to include):
- Id
- Username (unique)
- **Email (unique)** ? NEW!
- **PasswordHash** ?
- Role
- CreatedAt

## How to Apply the Changes

### Option 1: Run SQL Script (Fastest)
1. Open MySQL Workbench or your MySQL client
2. Connect to your database
3. Open the `update_database.sql` file
4. Execute it

### Option 2: Recreate Database (Clean slate)
The application will automatically recreate the database with the new schema on startup.

**Steps:**
1. Stop the API if it's running
2. Drop the existing database:
   ```sql
   DROP DATABASE IF EXISTS casino_db;
   CREATE DATABASE casino_db;
   ```
3. Run the API:
   ```powershell
   cd C:\Users\ahmad\source\repos\CasinoBackend\CasinoProject\Casino_Api
   dotnet run
   ```
4. The database will be created automatically with all tables including the Email columns

### Option 3: Manual MySQL Update
If you prefer doing it manually:

```sql
USE casino_db;

-- Add Email column to AdminUsers
ALTER TABLE `AdminUsers` 
ADD COLUMN `Email` VARCHAR(100) NOT NULL AFTER `Username`;

-- Add unique index
CREATE UNIQUE INDEX `IX_AdminUsers_Email` ON `AdminUsers`(`Email`);

-- Update the seeded admin user
UPDATE `AdminUsers` 
SET `Email` = 'admin@casinoapi.com' 
WHERE `Username` = 'admin';
```

## Verify the Changes

After applying the changes, verify both tables have email and password columns:

```sql
-- Check AdminUsers table structure
DESCRIBE AdminUsers;

-- Check Users table structure
DESCRIBE Users;

-- View admin users
SELECT Id, Username, Email, Role, CreatedAt FROM AdminUsers;

-- View regular users
SELECT Id, Username, Email, Balance, CreatedAt FROM Users;
```

## Default Credentials

### Admin User (AdminUsers table)
- Username: `admin`
- Email: `admin@casinoapi.com`
- Password: `Admin@123`
- Password is hashed using BCrypt

### Regular Users (Users table)
- Created through registration endpoint
- Each has username, email, and BCrypt hashed password

## Testing the API

Once the database is updated, you can test the authentication:

### Register a new user:
```bash
POST http://localhost:5001/api/auth/register?apiKey=default_tenant_api_key_12345
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test@123"
}
```

### Login with admin:
```bash
POST http://localhost:5001/api/auth/login?apiKey=default_tenant_api_key_12345
Content-Type: application/json

{
  "email": "admin@casinoapi.com",
  "password": "Admin@123"
}
```

## Viewing Data in MySQL

To see emails and hashed passwords for both tables:

```sql
-- View all admin users
SELECT 
    Id,
    Username,
Email,
    SUBSTRING(PasswordHash, 1, 20) as 'Password Hash (truncated)',
    Role,
  CreatedAt
FROM AdminUsers;

-- View all regular users
SELECT 
    Id,
    Username,
    Email,
    SUBSTRING(PasswordHash, 1, 20) as 'Password Hash (truncated)',
    Balance,
    CreatedAt
FROM Users;
```

## Password Hashing

Both tables use **BCrypt** for password hashing:
- Passwords are salted automatically
- Hashes are 60 characters long
- Format: `$2a$11$...` (BCrypt format)
- Secure against rainbow table attacks

## Next Steps

1. ? Models updated (User.cs and AdminUser.cs)
2. ? Database context configured (AppDbContext.cs)
3. ? **Apply database changes** (choose one option above)
4. ? **Test the API** with Swagger or HTTP client
5. ? **Verify data** in MySQL Workbench

