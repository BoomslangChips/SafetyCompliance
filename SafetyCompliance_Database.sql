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

CREATE NONCLUSTERED INDEX [IX_Plants_CompanyId]          ON [dbo].[Plants]([CompanyId])
GO
CREATE NONCLUSTERED INDEX [IX_Plants_CompanyId_IsActive]  ON [dbo].[Plants]([CompanyId], [IsActive])
GO
CREATE NONCLUSTERED INDEX [IX_Plants_IsActive]            ON [dbo].[Plants]([IsActive]) INCLUDE ([CompanyId])
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
CREATE NONCLUSTERED INDEX [IX_Sections_PlantId]           ON [dbo].[Sections]([PlantId])
GO
CREATE NONCLUSTERED INDEX [IX_Sections_PlantId_IsActive]  ON [dbo].[Sections]([PlantId], [IsActive])
GO
CREATE NONCLUSTERED INDEX [IX_Sections_IsActive]          ON [dbo].[Sections]([IsActive]) INCLUDE ([PlantId])
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
    [Id]                 INT IDENTITY(1,1) NOT NULL,
    [EquipmentTypeId]    INT NOT NULL,
    -- NULL = applies to all sub-types; set = applies only to this sub-type
    [EquipmentSubTypeId] INT NULL,
    [ItemName]           NVARCHAR(300) NOT NULL,
    [Description]        NVARCHAR(500) NULL,
    [SortOrder]          INT NOT NULL DEFAULT 0,
    [IsRequired]         BIT NOT NULL DEFAULT 1,
    [IsActive]           BIT NOT NULL DEFAULT 1,
    [CreatedAt]          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ChecklistItemTemplates] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ChecklistItems_Types] FOREIGN KEY ([EquipmentTypeId])
        REFERENCES [dbo].[EquipmentTypes]([Id]),
    CONSTRAINT [FK_ChecklistItems_SubTypes] FOREIGN KEY ([EquipmentSubTypeId])
        REFERENCES [dbo].[EquipmentSubTypes]([Id])
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
CREATE NONCLUSTERED INDEX [IX_Equipment_SectionId]           ON [dbo].[Equipment]([SectionId])
GO
CREATE NONCLUSTERED INDEX [IX_Equipment_SectionId_IsActive]  ON [dbo].[Equipment]([SectionId], [IsActive])
GO
CREATE NONCLUSTERED INDEX [IX_Equipment_IsActive]            ON [dbo].[Equipment]([IsActive]) INCLUDE ([SectionId])
GO
CREATE NONCLUSTERED INDEX [IX_Equipment_TypeId]              ON [dbo].[Equipment]([EquipmentTypeId])
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
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Fire Extinguishers', 'Portable fire suppression device')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Lay Flat Hose Box', 'Lay-flat hose in a wall-mounted cabinet')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Hose Reels', 'Wall-mounted hose reel unit')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Fire Hydrants', 'Fire hydrant point')
INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description]) VALUES ('Emergency Horns', 'Emergency sound horn station')
GO

-- Sub-Types for Fire Extinguishers (Type 1)
INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (1, '9Kg DCP')
INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (1, '5Kg CO2')
INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (1, '50L Foam Trolleys')
GO

-- Checklist for Fire Extinguishers - 9Kg DCP (Type 1, SubType 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Accessibility?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Fire Extinguisher Sign in Place and Clean?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Fire Extinguisher Arrow in Place and Clean?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Demarcation?', 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Seal in Place?', 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Nozzle Clean and Hose?', 6)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Pressure Gauge?', 7)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Fire Extinguisher Cover in Place?', 8)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 1, 'Service still up to Date?', 9)
GO

-- Checklist for Fire Extinguishers - 5Kg CO2 (Type 1, SubType 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Accessibility?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Fire Extinguisher Sign in Place and Clean?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Fire Extinguisher Arrow in Place and Clean?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Demarcation?', 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Seal in Place?', 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Nozzle Clean and Hose?', 6)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Fire Extinguisher Cover in Place?', 7)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 2, 'Service still up to Date?', 8)
GO

-- Checklist for Fire Extinguishers - 50L Foam Trolleys (Type 1, SubType 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Accessibility?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Fire Extinguisher Sign in Place and Clean?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Fire Extinguisher Arrow in Place and Clean?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Demarcation?', 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Seal in Place?', 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Nozzle Clean and Hose?', 6)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Fire Extinguisher Cover in Place?', 7)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (1, 3, 'Service still up to Date?', 8)
GO

-- Checklist for Lay Flat Hose Box (Type 2, no sub-type)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Lay Flat Hose Sign in Place and Clean?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Direction Arrow in Place and Clean?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Lay Flat Hose Cabinet in Place?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Lay Flat Hose Cabinet Seal in Place?', 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, '2 x Lay Flat Hoses in Cabinet?', 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, '1 x Branch Nozel in Place?', 6)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Lay Flat Hose Cabinet Damaged?', 7)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (2, 'Lay Flat Hose Cabinet free from Obstruction?', 8)
GO

-- Checklist for Hose Reels (Type 3, no sub-type)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Hose Reel Sign in Place and Clean?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Direction Arrow in Place and Clean?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Hose Reel Cover in Place?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Hose Reel Nozzle in Place?', 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Hose Reel Valve in Place?', 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Hose Reel Seal in place?', 6)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'All clean?', 7)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (3, 'Hose Reel free from Obstruction?', 8)
GO

-- Checklist for Fire Hydrants (Type 4, no sub-type)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Fire Hydrant Sign in Place and Clean?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Direction Arrow in Place and Clean?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'All clean?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Fire Hydrant free from Obstruction?', 4)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Fire Hydrant wheel in place?', 5)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Fire Hydrant Cap in place?', 6)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (4, 'Fire Hydrant seal in place?', 7)
GO

-- Checklist for Emergency Horns (Type 5, no sub-type)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (5, 'Sound Horn Sign in Place and Clean?', 1)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (5, 'Direction Arrow in Place and Clean?', 2)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (5, 'Sound Horn Cabinet in Place?', 3)
INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (5, 'Sound Horn Cabinet Seal in Place?', 4)
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
PRINT 'Seeded: 2 companies, 5 equipment types, 3 fire extinguisher sub-types, 52 checklist items'
GO
