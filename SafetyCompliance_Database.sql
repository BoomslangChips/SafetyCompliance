-- ============================================================
-- SafeCheck - Safety & Compliance System
-- Full Database Creation Script
-- Server: SQL2012\SQLEXPRESS | Auth: sa / smiles12
-- ============================================================

USE [master]
GO

IF DB_ID('SafetyCompliance') IS NOT NULL
BEGIN
    ALTER DATABASE [SafetyCompliance] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE [SafetyCompliance]
END
GO

CREATE DATABASE [SafetyCompliance]
GO

USE [SafetyCompliance]
GO

-- ============================================================
-- ASP.NET Identity Tables (EF will create these, but schema here for reference)
-- Run EF migrations first, then run this script for seed data.
-- OR create the full schema here if not using EF migrations.
-- ============================================================

-- ============================================================
-- Companies
-- ============================================================
CREATE TABLE [dbo].[Companies] (
    [Id]           INT IDENTITY(1,1) NOT NULL,
    [Name]         NVARCHAR(200) NOT NULL,
    [Code]         NVARCHAR(50) NULL,
    [Address]      NVARCHAR(500) NULL,
    [ContactName]  NVARCHAR(200) NULL,
    [ContactEmail] NVARCHAR(200) NULL,
    [ContactPhone] NVARCHAR(50) NULL,
    [IsActive]     BIT NOT NULL DEFAULT 1,
    [CreatedAt]    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedById]  NVARCHAR(450) NULL,
    [ModifiedAt]   DATETIME2 NULL,
    [ModifiedById] NVARCHAR(450) NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED ([Id])
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Companies_Code]
    ON [dbo].[Companies]([Code]) WHERE [Code] IS NOT NULL
GO

-- ============================================================
-- Plants (no Address - companies have addresses, not plants)
-- ============================================================
CREATE TABLE [dbo].[Plants] (
    [Id]            INT IDENTITY(1,1) NOT NULL,
    [CompanyId]     INT NOT NULL,
    [Name]          NVARCHAR(200) NOT NULL,
    [Description]   NVARCHAR(500) NULL,
    [ContactName]   NVARCHAR(200) NULL,
    [ContactPhone]  NVARCHAR(50) NULL,
    [ContactEmail]  NVARCHAR(200) NULL,
    [IsActive]      BIT NOT NULL DEFAULT 1,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedById]   NVARCHAR(450) NULL,
    [ModifiedAt]    DATETIME2 NULL,
    [ModifiedById]  NVARCHAR(450) NULL,
    CONSTRAINT [PK_Plants] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Plants_Companies] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[Companies]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_Plants_CompanyId] ON [dbo].[Plants]([CompanyId])
GO

-- ============================================================
-- Sections
-- ============================================================
CREATE TABLE [dbo].[Sections] (
    [Id]           INT IDENTITY(1,1) NOT NULL,
    [PlantId]      INT NOT NULL,
    [Name]         NVARCHAR(200) NOT NULL,
    [Description]  NVARCHAR(500) NULL,
    [SortOrder]    INT NOT NULL DEFAULT 0,
    [IsActive]     BIT NOT NULL DEFAULT 1,
    [CreatedAt]    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedById]  NVARCHAR(450) NULL,
    [ModifiedAt]   DATETIME2 NULL,
    [ModifiedById] NVARCHAR(450) NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Sections_Plants] FOREIGN KEY ([PlantId])
        REFERENCES [dbo].[Plants]([Id])
)
GO

-- ============================================================
-- Equipment Types (dynamic - users can add new types)
-- ============================================================
CREATE TABLE [dbo].[EquipmentTypes] (
    [Id]          INT IDENTITY(1,1) NOT NULL,
    [Name]        NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IconClass]   NVARCHAR(100) NULL,
    [IsActive]    BIT NOT NULL DEFAULT 1,
    [CreatedAt]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_EquipmentTypes] PRIMARY KEY CLUSTERED ([Id])
)
GO

-- ============================================================
-- Equipment Sub-Types (e.g. DCP, CO2, Foam for Fire Extinguishers)
-- ============================================================
CREATE TABLE [dbo].[EquipmentSubTypes] (
    [Id]              INT IDENTITY(1,1) NOT NULL,
    [EquipmentTypeId] INT NOT NULL,
    [Name]            NVARCHAR(200) NOT NULL,
    [IsActive]        BIT NOT NULL DEFAULT 1,
    [CreatedAt]       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_EquipmentSubTypes] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_EquipmentSubTypes_Types] FOREIGN KEY ([EquipmentTypeId])
        REFERENCES [dbo].[EquipmentTypes]([Id])
)
GO

-- ============================================================
-- Checklist Item Templates (per equipment type - dynamic)
-- ============================================================
CREATE TABLE [dbo].[ChecklistItemTemplates] (
    [Id]              INT IDENTITY(1,1) NOT NULL,
    [EquipmentTypeId] INT NOT NULL,
    [ItemName]        NVARCHAR(300) NOT NULL,
    [Description]     NVARCHAR(500) NULL,
    [SortOrder]       INT NOT NULL DEFAULT 0,
    [IsRequired]      BIT NOT NULL DEFAULT 1,
    [IsActive]        BIT NOT NULL DEFAULT 1,
    [CreatedAt]       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ChecklistItemTemplates] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ChecklistItems_Types] FOREIGN KEY ([EquipmentTypeId])
        REFERENCES [dbo].[EquipmentTypes]([Id])
)
GO

-- ============================================================
-- Equipment
-- ============================================================
CREATE TABLE [dbo].[Equipment] (
    [Id]                 INT IDENTITY(1,1) NOT NULL,
    [SectionId]          INT NOT NULL,
    [EquipmentTypeId]    INT NOT NULL,
    [EquipmentSubTypeId] INT NULL,
    [Identifier]         NVARCHAR(100) NOT NULL,
    [Description]        NVARCHAR(500) NULL,
    [Size]               NVARCHAR(100) NULL,
    [SerialNumber]       NVARCHAR(200) NULL,
    [InstallDate]        DATE NULL,
    [LastServiceDate]    DATE NULL,
    [NextServiceDate]    DATE NULL,
    [SortOrder]          INT NOT NULL DEFAULT 0,
    [IsActive]           BIT NOT NULL DEFAULT 1,
    [CreatedAt]          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedById]        NVARCHAR(450) NULL,
    [ModifiedAt]         DATETIME2 NULL,
    [ModifiedById]       NVARCHAR(450) NULL,
    CONSTRAINT [PK_Equipment] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Equipment_Sections] FOREIGN KEY ([SectionId])
        REFERENCES [dbo].[Sections]([Id]),
    CONSTRAINT [FK_Equipment_Types] FOREIGN KEY ([EquipmentTypeId])
        REFERENCES [dbo].[EquipmentTypes]([Id]),
    CONSTRAINT [FK_Equipment_SubTypes] FOREIGN KEY ([EquipmentSubTypeId])
        REFERENCES [dbo].[EquipmentSubTypes]([Id])
)
GO

-- ============================================================
-- Inspection Statuses (lookup)
-- ============================================================
CREATE TABLE [dbo].[InspectionStatuses] (
    [Id]   INT NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_InspectionStatuses] PRIMARY KEY CLUSTERED ([Id])
)
GO

INSERT INTO [dbo].[InspectionStatuses] VALUES (0, 'Pending'), (1, 'InProgress'), (2, 'Completed')
GO

-- ============================================================
-- Inspection Rounds
-- ============================================================
CREATE TABLE [dbo].[InspectionRounds] (
    [Id]              INT IDENTITY(1,1) NOT NULL,
    [PlantId]         INT NOT NULL,
    [InspectionDate]  DATE NOT NULL,
    [InspectionMonth] NVARCHAR(7) NOT NULL,
    [Status]          INT NOT NULL DEFAULT 0,
    [StartedAt]       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CompletedAt]     DATETIME2 NULL,
    [InspectedById]   NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_InspectionRounds] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Rounds_Plants] FOREIGN KEY ([PlantId])
        REFERENCES [dbo].[Plants]([Id]),
    CONSTRAINT [FK_Rounds_Status] FOREIGN KEY ([Status])
        REFERENCES [dbo].[InspectionStatuses]([Id])
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Rounds_PlantMonth]
    ON [dbo].[InspectionRounds]([PlantId], [InspectionMonth])
GO

-- ============================================================
-- Equipment Inspections (per equipment per round)
-- ============================================================
CREATE TABLE [dbo].[EquipmentInspections] (
    [Id]                INT IDENTITY(1,1) NOT NULL,
    [InspectionRoundId] INT NOT NULL,
    [EquipmentId]       INT NOT NULL,
    [IsComplete]        BIT NOT NULL DEFAULT 0,
    [Comments]          NVARCHAR(MAX) NULL,
    [InspectedAt]       DATETIME2 NULL,
    CONSTRAINT [PK_EquipmentInspections] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_EqInsp_Rounds] FOREIGN KEY ([InspectionRoundId])
        REFERENCES [dbo].[InspectionRounds]([Id]),
    CONSTRAINT [FK_EqInsp_Equipment] FOREIGN KEY ([EquipmentId])
        REFERENCES [dbo].[Equipment]([Id])
)
GO

-- ============================================================
-- Inspection Responses (checklist answers)
-- ============================================================
CREATE TABLE [dbo].[InspectionResponses] (
    [Id]                       INT IDENTITY(1,1) NOT NULL,
    [EquipmentInspectionId]    INT NOT NULL,
    [ChecklistItemTemplateId]  INT NOT NULL,
    [Response]                 BIT NULL,
    [Comment]                  NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_InspectionResponses] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Resp_EqInsp] FOREIGN KEY ([EquipmentInspectionId])
        REFERENCES [dbo].[EquipmentInspections]([Id]),
    CONSTRAINT [FK_Resp_Template] FOREIGN KEY ([ChecklistItemTemplateId])
        REFERENCES [dbo].[ChecklistItemTemplates]([Id])
)
GO

-- ============================================================
-- Inspection Photos
-- ============================================================
CREATE TABLE [dbo].[InspectionPhotos] (
    [Id]                    INT IDENTITY(1,1) NOT NULL,
    [EquipmentInspectionId] INT NOT NULL,
    [FileName]              NVARCHAR(500) NOT NULL,
    [FilePath]              NVARCHAR(1000) NOT NULL,
    [ContentType]           NVARCHAR(100) NOT NULL,
    [FileSizeBytes]         BIGINT NOT NULL,
    [UploadedAt]            DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UploadedById]          NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_InspectionPhotos] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Photos_EqInsp] FOREIGN KEY ([EquipmentInspectionId])
        REFERENCES [dbo].[EquipmentInspections]([Id])
)
GO

-- ============================================================
-- User-Company Association
-- ============================================================
CREATE TABLE [dbo].[UserCompanies] (
    [UserId]    NVARCHAR(450) NOT NULL,
    [CompanyId] INT NOT NULL,
    CONSTRAINT [PK_UserCompanies] PRIMARY KEY CLUSTERED ([UserId], [CompanyId]),
    CONSTRAINT [FK_UC_Companies] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[Companies]([Id]) ON DELETE CASCADE
)
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Companies
INSERT INTO [dbo].[Companies] ([Name], [Code]) VALUES ('Majesty Oil Mills', 'MOM')
INSERT INTO [dbo].[Companies] ([Name], [Code]) VALUES ('Majesty Oil Mills TVP', 'MOM-TVP')
GO

-- Equipment Types
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Fire Extinguisher', 'Portable fire suppression device')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Fire Hose Reel', 'Wall-mounted fire hose reel')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Emergency Light', 'Battery-backed emergency lighting')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Fire Alarm', 'Fire detection and alarm system')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('First Aid Kit', 'Emergency first aid supplies')
GO

-- Sub-Types for Fire Extinguisher
INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (1, 'DCP (Dry Chemical Powder)')
INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (1, 'CO2 (Carbon Dioxide)')
INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (1, 'Foam')
INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (1, 'Water')
GO

-- Checklist for Fire Extinguisher (Type 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the extinguisher accessible and not blocked?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the pressure gauge in the green zone?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the safety pin and tamper seal intact?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the extinguisher clean and free of damage?', 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the nozzle/hose in good condition?', 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the mounting bracket secure?', 6)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the inspection tag up to date?', 7)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (1, 'Is the operating instructions label legible?', 8)
GO

-- Checklist for Fire Hose Reel (Type 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Is the hose reel accessible?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Is the hose in good condition (no cracks/leaks)?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Does the nozzle operate correctly?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Is the valve operational?', 4)
GO

-- Checklist for Emergency Light (Type 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Does the light turn on during power test?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Is the unit clean and undamaged?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Is the battery charge indicator showing normal?', 3)
GO

-- Checklist for Fire Alarm (Type 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Is the alarm panel showing normal status?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Are all call points accessible?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Is the sounders/strobe test successful?', 3)
GO

-- Checklist for First Aid Kit (Type 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (5, 'Is the kit accessible and clearly marked?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (5, 'Are all items present and within expiry date?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (5, 'Is the container clean and sealed?', 3)
GO

-- ============================================================
-- VIEWS
-- ============================================================

CREATE VIEW [dbo].[vw_EquipmentOverview]
AS
SELECT
    e.[Id] AS EquipmentId,
    c.[Id] AS CompanyId,
    c.[Name] AS CompanyName,
    p.[Id] AS PlantId,
    p.[Name] AS PlantName,
    s.[Id] AS SectionId,
    s.[Name] AS SectionName,
    et.[Name] AS EquipmentType,
    est.[Name] AS SubType,
    e.[Identifier],
    e.[Description],
    e.[Size],
    e.[SerialNumber],
    e.[NextServiceDate],
    e.[IsActive]
FROM [dbo].[Equipment] e
INNER JOIN [dbo].[Sections] s ON e.[SectionId] = s.[Id]
INNER JOIN [dbo].[Plants] p ON s.[PlantId] = p.[Id]
INNER JOIN [dbo].[Companies] c ON p.[CompanyId] = c.[Id]
INNER JOIN [dbo].[EquipmentTypes] et ON e.[EquipmentTypeId] = et.[Id]
LEFT JOIN [dbo].[EquipmentSubTypes] est ON e.[EquipmentSubTypeId] = est.[Id]
GO

PRINT 'SafetyCompliance database created successfully.'
PRINT 'Seeded: 2 companies, 5 equipment types, 4 fire extinguisher sub-types, 21 checklist items'
GO
