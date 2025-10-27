-- Add nullable owner_user_id column and foreign key to dbo.tb_users
-- Safe to run multiple times; checks for existing column/constraint.

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE [object_id] = OBJECT_ID(N'dbo.Sandwiches') AND name = N'owner_user_id')
BEGIN
    ALTER TABLE dbo.Sandwiches ADD owner_user_id INT NULL;
END

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys fk
    WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Sandwiches')
      AND fk.referenced_object_id = OBJECT_ID(N'dbo.tb_users')
      AND fk.name = N'FK_Sandwiches_tb_users_owner_user_id'
)
BEGIN
    -- Only attempt to add FK if tb_users exists
    IF OBJECT_ID(N'dbo.tb_users') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.Sandwiches WITH CHECK ADD CONSTRAINT FK_Sandwiches_tb_users_owner_user_id FOREIGN KEY(owner_user_id)
            REFERENCES dbo.tb_users(Id);
        ALTER TABLE dbo.Sandwiches CHECK CONSTRAINT FK_Sandwiches_tb_users_owner_user_id;
    END
END

COMMIT TRANSACTION;

-- Notes:
-- Run this against your SQL Server (Docker) database where the scaffolded DbContext points.
-- Example using sqlcmd:
-- sqlcmd -S <server> -U <user> -P <password> -d <database> -i db/scripts/0001_add_owner_user_id.sql
