-- Add is_private column to Sandwiches table and set default to 0 (public)
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE [object_id] = OBJECT_ID(N'dbo.Sandwiches') AND name = N'is_private')
BEGIN
    ALTER TABLE dbo.Sandwiches ADD is_private BIT NOT NULL CONSTRAINT DF_Sandwiches_IsPrivate DEFAULT(0);
END

COMMIT TRANSACTION;

-- Note: run this against your SQL Server instance used by the Docker scaffolded context.