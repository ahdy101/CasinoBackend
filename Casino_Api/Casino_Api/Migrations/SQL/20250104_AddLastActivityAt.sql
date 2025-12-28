-- Add LastActivityAt column to Users table
ALTER TABLE Users ADD COLUMN LastActivityAt DATETIME NULL;

-- Create index for querying active users
CREATE INDEX IX_Users_LastActivityAt ON Users(LastActivityAt);

-- Update existing users to current time
UPDATE Users SET LastActivityAt = NOW() WHERE IsDeleted = 0;
