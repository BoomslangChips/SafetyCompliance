-- ============================================================
-- SafeCheck - Reset Equipment Types, SubTypes & Checklists
-- Removes ALL existing equipment-related data and replaces
-- with new checklist structure
-- ============================================================

USE [SafetyCompliance]
GO

-- ============================================================
-- STEP 1: Delete all existing data (FK order matters)
-- ============================================================

-- Delete inspection responses (references ChecklistItemTemplates + EquipmentInspections)
DELETE FROM [dbo].[InspectionResponses]
GO

-- Delete inspection photos (references EquipmentInspections)
DELETE FROM [dbo].[InspectionPhotos]
GO

-- Delete comments linked to inspection rounds
DELETE FROM [dbo].[Comments]
GO

-- Delete issues (references Equipment + EquipmentInspections + InspectionRounds)
DELETE FROM [dbo].[Issues]
GO

-- Delete equipment inspections (references Equipment + InspectionRounds)
DELETE FROM [dbo].[EquipmentInspections]
GO

-- Delete inspection rounds
DELETE FROM [dbo].[InspectionRounds]
GO

-- Delete inspection schedules
DELETE FROM [dbo].[InspectionSchedules]
GO

-- Delete service bookings (references Equipment)
DELETE FROM [dbo].[ServiceBookings]
GO

-- Delete notes linked to equipment
UPDATE [dbo].[Notes] SET [EquipmentId] = NULL WHERE [EquipmentId] IS NOT NULL
GO

-- Delete all equipment
DELETE FROM [dbo].[Equipment]
GO

-- Delete all checklist item templates
DELETE FROM [dbo].[ChecklistItemTemplates]
GO

-- Delete all equipment sub-types
DELETE FROM [dbo].[EquipmentSubTypes]
GO

-- Delete all equipment types
DELETE FROM [dbo].[EquipmentTypes]
GO

-- Reset identity seeds
DBCC CHECKIDENT ('EquipmentTypes', RESEED, 0)
GO
DBCC CHECKIDENT ('EquipmentSubTypes', RESEED, 0)
GO
DBCC CHECKIDENT ('ChecklistItemTemplates', RESEED, 0)
GO
DBCC CHECKIDENT ('Equipment', RESEED, 0)
GO

-- ============================================================
-- STEP 2: Insert new Equipment Types
-- ============================================================
SET IDENTITY_INSERT [dbo].[EquipmentTypes] ON
GO

INSERT [dbo].[EquipmentTypes] ([Id], [Name], [Description], [IconClass], [IsActive], [CreatedAt]) VALUES
(1, N'Fire Extinguishers', N'Portable fire suppression device', NULL, 1, GETUTCDATE()),
(2, N'Lay Flat Hose Box', N'Lay-flat hose in a wall-mounted cabinet', NULL, 1, GETUTCDATE()),
(3, N'Hose Reels', N'Wall-mounted hose reel unit', NULL, 1, GETUTCDATE()),
(4, N'Fire Hydrants', N'Fire hydrant point', NULL, 1, GETUTCDATE()),
(5, N'Emergency Horns', N'Emergency sound horn station', NULL, 1, GETUTCDATE())
GO

SET IDENTITY_INSERT [dbo].[EquipmentTypes] OFF
GO

-- ============================================================
-- STEP 3: Insert new Equipment Sub-Types
-- ============================================================
SET IDENTITY_INSERT [dbo].[EquipmentSubTypes] ON
GO

INSERT [dbo].[EquipmentSubTypes] ([Id], [EquipmentTypeId], [Name], [IsActive], [CreatedAt]) VALUES
(1, 1, N'9Kg DCP', 1, GETUTCDATE()),
(2, 1, N'5Kg CO2', 1, GETUTCDATE()),
(3, 1, N'50L Foam Trolleys', 1, GETUTCDATE())
GO

SET IDENTITY_INSERT [dbo].[EquipmentSubTypes] OFF
GO

-- ============================================================
-- STEP 4: Insert Checklist Item Templates
-- ============================================================
SET IDENTITY_INSERT [dbo].[ChecklistItemTemplates] ON
GO

-- -------------------------------------------------------
-- Fire Extinguishers 9Kg DCP (EquipmentTypeId=1, SubTypeId=1)
-- -------------------------------------------------------
INSERT [dbo].[ChecklistItemTemplates] ([Id], [EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [Description], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(1,  1, 1, N'Accessibility?', NULL, 1, 1, 1, GETUTCDATE()),
(2,  1, 1, N'Fire Extinguisher Sign in Place and Clean?', NULL, 2, 1, 1, GETUTCDATE()),
(3,  1, 1, N'Fire Extinguisher Arrow in Place and Clean?', NULL, 3, 1, 1, GETUTCDATE()),
(4,  1, 1, N'Demarcation?', NULL, 4, 1, 1, GETUTCDATE()),
(5,  1, 1, N'Seal in Place?', NULL, 5, 1, 1, GETUTCDATE()),
(6,  1, 1, N'Nozzle Clean and Hose?', NULL, 6, 1, 1, GETUTCDATE()),
(7,  1, 1, N'Pressure Gauge?', NULL, 7, 1, 1, GETUTCDATE()),
(8,  1, 1, N'Fire Extinguisher Cover in Place?', NULL, 8, 1, 1, GETUTCDATE()),
(9,  1, 1, N'Service still up to Date?', NULL, 9, 1, 1, GETUTCDATE())
GO

-- -------------------------------------------------------
-- Fire Extinguishers 5Kg CO2 (EquipmentTypeId=1, SubTypeId=2)
-- -------------------------------------------------------
INSERT [dbo].[ChecklistItemTemplates] ([Id], [EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [Description], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(10, 1, 2, N'Accessibility?', NULL, 1, 1, 1, GETUTCDATE()),
(11, 1, 2, N'Fire Extinguisher Sign in Place and Clean?', NULL, 2, 1, 1, GETUTCDATE()),
(12, 1, 2, N'Fire Extinguisher Arrow in Place and Clean?', NULL, 3, 1, 1, GETUTCDATE()),
(13, 1, 2, N'Demarcation?', NULL, 4, 1, 1, GETUTCDATE()),
(14, 1, 2, N'Seal in Place?', NULL, 5, 1, 1, GETUTCDATE()),
(15, 1, 2, N'Nozzle Clean and Hose?', NULL, 6, 1, 1, GETUTCDATE()),
(16, 1, 2, N'Fire Extinguisher Cover in Place?', NULL, 7, 1, 1, GETUTCDATE()),
(17, 1, 2, N'Service still up to Date?', NULL, 8, 1, 1, GETUTCDATE())
GO

-- -------------------------------------------------------
-- Fire Extinguishers 50L Foam Trolleys (EquipmentTypeId=1, SubTypeId=3)
-- -------------------------------------------------------
INSERT [dbo].[ChecklistItemTemplates] ([Id], [EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [Description], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(18, 1, 3, N'Accessibility?', NULL, 1, 1, 1, GETUTCDATE()),
(19, 1, 3, N'Fire Extinguisher Sign in Place and Clean?', NULL, 2, 1, 1, GETUTCDATE()),
(20, 1, 3, N'Fire Extinguisher Arrow in Place and Clean?', NULL, 3, 1, 1, GETUTCDATE()),
(21, 1, 3, N'Demarcation?', NULL, 4, 1, 1, GETUTCDATE()),
(22, 1, 3, N'Seal in Place?', NULL, 5, 1, 1, GETUTCDATE()),
(23, 1, 3, N'Nozzle Clean and Hose?', NULL, 6, 1, 1, GETUTCDATE()),
(24, 1, 3, N'Fire Extinguisher Cover in Place?', NULL, 7, 1, 1, GETUTCDATE()),
(25, 1, 3, N'Service still up to Date?', NULL, 8, 1, 1, GETUTCDATE())
GO

-- -------------------------------------------------------
-- Lay Flat Hose Box (EquipmentTypeId=2, no sub-type)
-- -------------------------------------------------------
INSERT [dbo].[ChecklistItemTemplates] ([Id], [EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [Description], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(26, 2, NULL, N'Lay Flat Hose Sign in Place and Clean?', NULL, 1, 1, 1, GETUTCDATE()),
(27, 2, NULL, N'Direction Arrow in Place and Clean?', NULL, 2, 1, 1, GETUTCDATE()),
(28, 2, NULL, N'Lay Flat Hose Cabinet in Place?', NULL, 3, 1, 1, GETUTCDATE()),
(29, 2, NULL, N'Lay Flat Hose Cabinet Seal in Place?', NULL, 4, 1, 1, GETUTCDATE()),
(30, 2, NULL, N'2 x Lay Flat Hoses in Cabinet?', NULL, 5, 1, 1, GETUTCDATE()),
(31, 2, NULL, N'1 x Branch Nozel in Place?', NULL, 6, 1, 1, GETUTCDATE()),
(32, 2, NULL, N'Lay Flat Hose Cabinet Damaged?', NULL, 7, 1, 1, GETUTCDATE()),
(33, 2, NULL, N'Lay Flat Hose Cabinet free from Obstruction?', NULL, 8, 1, 1, GETUTCDATE())
GO

-- -------------------------------------------------------
-- Hose Reels (EquipmentTypeId=3, no sub-type)
-- -------------------------------------------------------
INSERT [dbo].[ChecklistItemTemplates] ([Id], [EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [Description], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(34, 3, NULL, N'Hose Reel Sign in Place and Clean?', NULL, 1, 1, 1, GETUTCDATE()),
(35, 3, NULL, N'Direction Arrow in Place and Clean?', NULL, 2, 1, 1, GETUTCDATE()),
(36, 3, NULL, N'Hose Reel Cover in Place?', NULL, 3, 1, 1, GETUTCDATE()),
(37, 3, NULL, N'Hose Reel Nozzle in Place?', NULL, 4, 1, 1, GETUTCDATE()),
(38, 3, NULL, N'Hose Reel Valve in Place?', NULL, 5, 1, 1, GETUTCDATE()),
(39, 3, NULL, N'Hose Reel Seal in place?', NULL, 6, 1, 1, GETUTCDATE()),
(40, 3, NULL, N'All clean?', NULL, 7, 1, 1, GETUTCDATE()),
(41, 3, NULL, N'Hose Reel free from Obstruction?', NULL, 8, 1, 1, GETUTCDATE())
GO

-- -------------------------------------------------------
-- Fire Hydrants (EquipmentTypeId=4, no sub-type)
-- -------------------------------------------------------
INSERT [dbo].[ChecklistItemTemplates] ([Id], [EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [Description], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(42, 4, NULL, N'Fire Hydrant Sign in Place and Clean?', NULL, 1, 1, 1, GETUTCDATE()),
(43, 4, NULL, N'Direction Arrow in Place and Clean?', NULL, 2, 1, 1, GETUTCDATE()),
(44, 4, NULL, N'All clean?', NULL, 3, 1, 1, GETUTCDATE()),
(45, 4, NULL, N'Fire Hydrant free from Obstruction?', NULL, 4, 1, 1, GETUTCDATE()),
(46, 4, NULL, N'Fire Hydrant wheel in place?', NULL, 5, 1, 1, GETUTCDATE()),
(47, 4, NULL, N'Fire Hydrant Cap in place?', NULL, 6, 1, 1, GETUTCDATE()),
(48, 4, NULL, N'Fire Hydrant seal in place?', NULL, 7, 1, 1, GETUTCDATE())
GO

-- -------------------------------------------------------
-- Emergency Horns (EquipmentTypeId=5, no sub-type)
-- -------------------------------------------------------
INSERT [dbo].[ChecklistItemTemplates] ([Id], [EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [Description], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(49, 5, NULL, N'Sound Horn Sign in Place and Clean?', NULL, 1, 1, 1, GETUTCDATE()),
(50, 5, NULL, N'Direction Arrow in Place and Clean?', NULL, 2, 1, 1, GETUTCDATE()),
(51, 5, NULL, N'Sound Horn Cabinet in Place?', NULL, 3, 1, 1, GETUTCDATE()),
(52, 5, NULL, N'Sound Horn Cabinet Seal in Place?', NULL, 4, 1, 1, GETUTCDATE())
GO

SET IDENTITY_INSERT [dbo].[ChecklistItemTemplates] OFF
GO

-- ============================================================
-- VERIFICATION: Show the new data
-- ============================================================
PRINT '=== Equipment Types ==='
SELECT [Id], [Name], [Description], [IsActive] FROM [dbo].[EquipmentTypes] ORDER BY [Id]

PRINT '=== Equipment Sub-Types ==='
SELECT st.[Id], t.[Name] AS TypeName, st.[Name] AS SubTypeName, st.[IsActive]
FROM [dbo].[EquipmentSubTypes] st
INNER JOIN [dbo].[EquipmentTypes] t ON st.[EquipmentTypeId] = t.[Id]
ORDER BY st.[Id]

PRINT '=== Checklist Item Templates ==='
SELECT c.[Id], t.[Name] AS TypeName, st.[Name] AS SubTypeName, c.[ItemName], c.[SortOrder]
FROM [dbo].[ChecklistItemTemplates] c
INNER JOIN [dbo].[EquipmentTypes] t ON c.[EquipmentTypeId] = t.[Id]
LEFT JOIN [dbo].[EquipmentSubTypes] st ON c.[EquipmentSubTypeId] = st.[Id]
ORDER BY t.[Id], ISNULL(st.[Id], 0), c.[SortOrder]
GO
