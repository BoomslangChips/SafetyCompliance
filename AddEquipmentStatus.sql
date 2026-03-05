-- ============================================================
-- SafeCheck - Add Equipment Status Column
-- Adds Status field for tracking equipment condition
-- (InService=0, OutOfService=1, Damaged=2, Missing=3,
--  NeedsReplacement=4, Retired=5)
-- ============================================================

USE [SafetyCompliance]
GO

-- Add Status column (defaults all existing to InService = 0)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'Status')
BEGIN
    ALTER TABLE [dbo].[Equipment] ADD [Status] TINYINT NOT NULL DEFAULT 0
END
GO

-- Index for filtering by status
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Equipment_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Equipment_Status]
        ON [dbo].[Equipment]([Status])
END
GO

PRINT 'Equipment status column added successfully.'
GO
