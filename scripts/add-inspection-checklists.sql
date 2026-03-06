-- ============================================================
--  SafetyCompliance - Add Inspection Checklist Items
--
--  This script adds all inspection checklist items for:
--    1. Fire Extinguishers 9Kg DCP      (TypeId 1, SubType NULL)
--    2. Fire Extinguishers 5Kg Co2      (TypeId 1, SubType 1)
--    3. Fire Extinguishers 50L Foam     (TypeId 1, NEW SubType)
--    4. Hose Reels                      (TypeId 2)
--    5. Emergency Horns / Alarm Station (TypeId 3)
--    6. Lay Flat Hose Boxes             (TypeId 4)
--    7. Fire Hydrants                   (NEW EquipmentType)
--
--  Also creates:
--    - New EquipmentType "Fire Hydrants"
--    - New EquipmentSubType "50L Foam Trolleys"
-- ============================================================

USE [SafetyCompliance];
GO

BEGIN TRANSACTION;

-- ────────────────────────────────────────────────────────────
-- STEP 1: Create new EquipmentType "Fire Hydrants"
-- ────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentTypes] WHERE [Name] = N'Fire Hydrants')
BEGIN
    INSERT INTO [dbo].[EquipmentTypes] ([Name], [Description], [IconClass], [IsActive], [CreatedAt])
    VALUES (N'Fire Hydrants', N'Fire Hydrant Points', NULL, 1, GETUTCDATE());
END;

DECLARE @TypeFireExtinguishers INT = (SELECT [Id] FROM [dbo].[EquipmentTypes] WHERE [Name] = N'Fire Extinguishers');
DECLARE @TypeHoseReels         INT = (SELECT [Id] FROM [dbo].[EquipmentTypes] WHERE [Name] = N'Hose Reel');
DECLARE @TypeAlarmStation      INT = (SELECT [Id] FROM [dbo].[EquipmentTypes] WHERE [Name] = N'Alarm Station');
DECLARE @TypeLayFlatHose       INT = (SELECT [Id] FROM [dbo].[EquipmentTypes] WHERE [Name] = N'Lay Flat Hose Boxes');
DECLARE @TypeFireHydrants      INT = (SELECT [Id] FROM [dbo].[EquipmentTypes] WHERE [Name] = N'Fire Hydrants');

-- ────────────────────────────────────────────────────────────
-- STEP 2: Create new EquipmentSubType "50L Foam Trolleys"
-- ────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = @TypeFireExtinguishers AND [Name] = N'50L Foam Trolleys')
BEGIN
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name], [IsActive], [CreatedAt])
    VALUES (@TypeFireExtinguishers, N'50L Foam Trolleys', 1, GETUTCDATE());
END;

DECLARE @SubTypeCo2        INT = (SELECT [Id] FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = @TypeFireExtinguishers AND [Name] = N'5Kg Co2');
DECLARE @SubTypeFoam       INT = (SELECT [Id] FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = @TypeFireExtinguishers AND [Name] = N'50L Foam Trolleys');

-- ────────────────────────────────────────────────────────────
-- STEP 3: Clear existing checklist items (fresh start)
-- ────────────────────────────────────────────────────────────

DELETE FROM [dbo].[ChecklistItemTemplates]
WHERE [EquipmentTypeId] IN (@TypeFireExtinguishers, @TypeHoseReels, @TypeAlarmStation, @TypeLayFlatHose, @TypeFireHydrants);

-- ────────────────────────────────────────────────────────────
-- STEP 4: Fire Extinguishers 9Kg DCP  (SubType = NULL)
--         9 items - applies to all fire extinguishers without
--         a subtype-specific checklist
-- ────────────────────────────────────────────────────────────

INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(@TypeFireExtinguishers, NULL, N'Accessibility?', 1, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Fire Extinguisher Sign in Place and Clean?', 2, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Fire Extinguisher Arrow in Place and Clean?', 3, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Demarcation?', 4, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Seal in Place?', 5, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Nozzle Clean and Hose?', 6, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Pressure Gauge?', 7, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Fire Extinguisher Cover in Place?', 8, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, NULL, N'Service still up to Date?', 9, 1, 1, GETUTCDATE());

-- ────────────────────────────────────────────────────────────
-- STEP 5: Fire Extinguishers 5Kg Co2  (SubType = Co2)
--         8 items - NO Pressure Gauge
-- ────────────────────────────────────────────────────────────

INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(@TypeFireExtinguishers, @SubTypeCo2, N'Accessibility?', 1, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeCo2, N'Fire Extinguisher Sign in Place and Clean?', 2, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeCo2, N'Fire Extinguisher Arrow in Place and Clean?', 3, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeCo2, N'Demarcation?', 4, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeCo2, N'Seal in Place?', 5, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeCo2, N'Nozzle Clean and Hose?', 6, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeCo2, N'Fire Extinguisher Cover in Place?', 7, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeCo2, N'Service still up to Date?', 8, 1, 1, GETUTCDATE());

-- ────────────────────────────────────────────────────────────
-- STEP 6: Fire Extinguishers 50L Foam Trolleys (SubType = Foam)
--         8 items - NO Pressure Gauge (same as Co2)
-- ────────────────────────────────────────────────────────────

INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(@TypeFireExtinguishers, @SubTypeFoam, N'Accessibility?', 1, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeFoam, N'Fire Extinguisher Sign in Place and Clean?', 2, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeFoam, N'Fire Extinguisher Arrow in Place and Clean?', 3, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeFoam, N'Demarcation?', 4, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeFoam, N'Seal in Place?', 5, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeFoam, N'Nozzle Clean and Hose?', 6, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeFoam, N'Fire Extinguisher Cover in Place?', 7, 1, 1, GETUTCDATE()),
(@TypeFireExtinguishers, @SubTypeFoam, N'Service still up to Date?', 8, 1, 1, GETUTCDATE());

-- ────────────────────────────────────────────────────────────
-- STEP 7: Lay Flat Hose Boxes  (TypeId 4)
--         8 items
-- ────────────────────────────────────────────────────────────

INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(@TypeLayFlatHose, NULL, N'Lay Flat Hose Sign in Place and Clean?', 1, 1, 1, GETUTCDATE()),
(@TypeLayFlatHose, NULL, N'Direction Arrow in Place and Clean?', 2, 1, 1, GETUTCDATE()),
(@TypeLayFlatHose, NULL, N'Lay Flat Hose Cabinet in Place?', 3, 1, 1, GETUTCDATE()),
(@TypeLayFlatHose, NULL, N'Lay Flat Hose Cabinet Seal in Place?', 4, 1, 1, GETUTCDATE()),
(@TypeLayFlatHose, NULL, N'2 x Lay Flat Hoses in Cabinet?', 5, 1, 1, GETUTCDATE()),
(@TypeLayFlatHose, NULL, N'1 x Branch Nozzle in Place?', 6, 1, 1, GETUTCDATE()),
(@TypeLayFlatHose, NULL, N'Lay Flat Hose Cabinet Damaged?', 7, 1, 1, GETUTCDATE()),
(@TypeLayFlatHose, NULL, N'Lay Flat Hose Cabinet free from Obstruction?', 8, 1, 1, GETUTCDATE());

-- ────────────────────────────────────────────────────────────
-- STEP 8: Hose Reels  (TypeId 2)
--         8 items
-- ────────────────────────────────────────────────────────────

INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(@TypeHoseReels, NULL, N'Hose Reel Sign in Place and Clean?', 1, 1, 1, GETUTCDATE()),
(@TypeHoseReels, NULL, N'Direction Arrow in Place and Clean?', 2, 1, 1, GETUTCDATE()),
(@TypeHoseReels, NULL, N'Hose Reel Cover in Place?', 3, 1, 1, GETUTCDATE()),
(@TypeHoseReels, NULL, N'Hose Reel Nozzle in Place?', 4, 1, 1, GETUTCDATE()),
(@TypeHoseReels, NULL, N'Hose Reel Valve in Place?', 5, 1, 1, GETUTCDATE()),
(@TypeHoseReels, NULL, N'Hose Reel Seal in Place?', 6, 1, 1, GETUTCDATE()),
(@TypeHoseReels, NULL, N'All Clean?', 7, 1, 1, GETUTCDATE()),
(@TypeHoseReels, NULL, N'Hose Reel free from Obstruction?', 8, 1, 1, GETUTCDATE());

-- ────────────────────────────────────────────────────────────
-- STEP 9: Fire Hydrants  (NEW EquipmentType)
--         7 items
-- ────────────────────────────────────────────────────────────

INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(@TypeFireHydrants, NULL, N'Fire Hydrant Sign in Place and Clean?', 1, 1, 1, GETUTCDATE()),
(@TypeFireHydrants, NULL, N'Direction Arrow in Place and Clean?', 2, 1, 1, GETUTCDATE()),
(@TypeFireHydrants, NULL, N'All Clean?', 3, 1, 1, GETUTCDATE()),
(@TypeFireHydrants, NULL, N'Fire Hydrant free from Obstruction?', 4, 1, 1, GETUTCDATE()),
(@TypeFireHydrants, NULL, N'Fire Hydrant Wheel in Place?', 5, 1, 1, GETUTCDATE()),
(@TypeFireHydrants, NULL, N'Fire Hydrant Cap in Place?', 6, 1, 1, GETUTCDATE()),
(@TypeFireHydrants, NULL, N'Fire Hydrant Seal in Place?', 7, 1, 1, GETUTCDATE());

-- ────────────────────────────────────────────────────────────
-- STEP 10: Emergency Horns / Alarm Station  (TypeId 3)
--          4 items
-- ────────────────────────────────────────────────────────────

INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder], [IsRequired], [IsActive], [CreatedAt]) VALUES
(@TypeAlarmStation, NULL, N'Sound Horn Sign in Place and Clean?', 1, 1, 1, GETUTCDATE()),
(@TypeAlarmStation, NULL, N'Direction Arrow in Place and Clean?', 2, 1, 1, GETUTCDATE()),
(@TypeAlarmStation, NULL, N'Sound Horn Cabinet in Place?', 3, 1, 1, GETUTCDATE()),
(@TypeAlarmStation, NULL, N'Sound Horn Cabinet Seal in Place?', 4, 1, 1, GETUTCDATE());

-- ────────────────────────────────────────────────────────────
-- VERIFICATION
-- ────────────────────────────────────────────────────────────

SELECT
    et.[Name]  AS [EquipmentType],
    st.[Name]  AS [SubType],
    t.[ItemName],
    t.[SortOrder]
FROM   [dbo].[ChecklistItemTemplates] t
INNER JOIN [dbo].[EquipmentTypes]    et ON et.[Id] = t.[EquipmentTypeId]
LEFT  JOIN [dbo].[EquipmentSubTypes] st ON st.[Id] = t.[EquipmentSubTypeId]
ORDER BY et.[Name], ISNULL(st.[Name], ''), t.[SortOrder];

-- ────────────────────────────────────────────────────────────
-- If results look correct:  COMMIT;
-- Otherwise:                ROLLBACK;
-- ────────────────────────────────────────────────────────────
-- COMMIT;
-- ROLLBACK;
