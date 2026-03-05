-- ============================================================
-- SafeCheck - Add Inventory Support & Compliance Checks
-- Makes Equipment.SectionId nullable (inventory pool)
-- Adds EquipmentChecks + EquipmentCheckRecords tables
-- ============================================================

USE [SafetyCompliance]
GO

-- ============================================================
-- STEP 1: Make Equipment.SectionId nullable (inventory support)
-- ============================================================

-- Drop the existing FK constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Equipment_Sections')
    ALTER TABLE [dbo].[Equipment] DROP CONSTRAINT [FK_Equipment_Sections]
GO

-- Make SectionId nullable
ALTER TABLE [dbo].[Equipment] ALTER COLUMN [SectionId] INT NULL
GO

-- Recreate FK with SET NULL delete behavior
ALTER TABLE [dbo].[Equipment] ADD CONSTRAINT [FK_Equipment_Sections]
    FOREIGN KEY ([SectionId]) REFERENCES [dbo].[Sections]([Id])
    ON DELETE SET NULL
GO

-- ============================================================
-- STEP 2: Create EquipmentChecks table (compliance check templates)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EquipmentChecks')
BEGIN
    CREATE TABLE [dbo].[EquipmentChecks] (
        [Id]                 INT IDENTITY(1,1) NOT NULL,
        [EquipmentTypeId]    INT NOT NULL,
        [EquipmentSubTypeId] INT NULL,
        [Name]               NVARCHAR(300) NOT NULL,
        [Description]        NVARCHAR(500) NULL,
        [IntervalMonths]     INT NULL,
        [IsRequired]         BIT NOT NULL DEFAULT 1,
        [SortOrder]          INT NOT NULL DEFAULT 0,
        [IsActive]           BIT NOT NULL DEFAULT 1,
        [CreatedAt]          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_EquipmentChecks] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_EquipmentChecks_Types] FOREIGN KEY ([EquipmentTypeId])
            REFERENCES [dbo].[EquipmentTypes]([Id]),
        CONSTRAINT [FK_EquipmentChecks_SubTypes] FOREIGN KEY ([EquipmentSubTypeId])
            REFERENCES [dbo].[EquipmentSubTypes]([Id])
    )

    CREATE NONCLUSTERED INDEX [IX_EquipmentChecks_EquipmentTypeId]
        ON [dbo].[EquipmentChecks]([EquipmentTypeId])
    CREATE NONCLUSTERED INDEX [IX_EquipmentChecks_EquipmentSubTypeId]
        ON [dbo].[EquipmentChecks]([EquipmentSubTypeId])
END
GO

-- ============================================================
-- STEP 3: Create EquipmentCheckRecords table (actual date values)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EquipmentCheckRecords')
BEGIN
    CREATE TABLE [dbo].[EquipmentCheckRecords] (
        [Id]               INT IDENTITY(1,1) NOT NULL,
        [EquipmentId]      INT NOT NULL,
        [EquipmentCheckId] INT NOT NULL,
        [DateValue]        DATE NOT NULL,
        [ExpiryDate]       DATE NULL,
        [Notes]            NVARCHAR(500) NULL,
        [CreatedAt]        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById]      NVARCHAR(450) NULL,
        [ModifiedAt]       DATETIME2 NULL,
        [ModifiedById]     NVARCHAR(450) NULL,
        CONSTRAINT [PK_EquipmentCheckRecords] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_CheckRecords_Equipment] FOREIGN KEY ([EquipmentId])
            REFERENCES [dbo].[Equipment]([Id]),
        CONSTRAINT [FK_CheckRecords_Checks] FOREIGN KEY ([EquipmentCheckId])
            REFERENCES [dbo].[EquipmentChecks]([Id])
    )

    CREATE NONCLUSTERED INDEX [IX_EquipmentCheckRecords_EquipmentId]
        ON [dbo].[EquipmentCheckRecords]([EquipmentId])
    CREATE NONCLUSTERED INDEX [IX_EquipmentCheckRecords_EquipmentCheckId]
        ON [dbo].[EquipmentCheckRecords]([EquipmentCheckId])
    CREATE UNIQUE NONCLUSTERED INDEX [IX_EquipmentCheckRecords_Equipment_Check]
        ON [dbo].[EquipmentCheckRecords]([EquipmentId], [EquipmentCheckId])
END
GO

-- ============================================================
-- STEP 4: Seed compliance checks for existing equipment types
-- ============================================================

-- Fire Extinguishers (Type 1) - 9Kg DCP (SubType 1)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 1, N'Pressure Test', N'Hydrostatic pressure test', 36, 1)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 1, N'Manufacturing Date', N'Date of manufacture', NULL, 2)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 1, N'Service Date', N'Last professional service', 12, 3)
GO

-- Fire Extinguishers (Type 1) - 5Kg CO2 (SubType 2)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 2, N'Pressure Test', N'Hydrostatic pressure test', 36, 1)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 2, N'Manufacturing Date', N'Date of manufacture', NULL, 2)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 2, N'Service Date', N'Last professional service', 12, 3)
GO

-- Fire Extinguishers (Type 1) - 50L Foam Trolleys (SubType 3)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 3, N'Pressure Test', N'Hydrostatic pressure test', 36, 1)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 3, N'Manufacturing Date', N'Date of manufacture', NULL, 2)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [EquipmentSubTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (1, 3, N'Service Date', N'Last professional service', 12, 3)
GO

-- Hose Reels (Type 3) - general (no subtype)
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (3, N'Service Date', N'Last professional service', 12, 1)
GO

-- Fire Hydrants (Type 4) - general
INSERT INTO [dbo].[EquipmentChecks] ([EquipmentTypeId], [Name], [Description], [IntervalMonths], [SortOrder])
VALUES (4, N'Service Date', N'Last professional service', 12, 1)
GO

PRINT 'Inventory support and compliance checks added successfully.'
GO
