-- ============================================================
--  SafetyCompliance – Add sub-type-specific checklists
--  Run this AFTER FixLiveData.sql has already been committed.
--
--  What this script does:
--  1. Adds EquipmentSubTypeId column to ChecklistItemTemplates
--  2. Adds the FK constraint and index
--  3. Inserts CO2-specific checklist items for both CO2 sub-types
--     (5kg CO2 and 2kg CO2) under TypeId = 8 (Fire Extinguishers)
--
--  The existing 8 DCP checklist items stay with
--  EquipmentSubTypeId = NULL, meaning they apply to ALL sub-types
--  that don't have their own specific set.  CO2 extinguishers get
--  their own set (weight check, horn check — no pressure gauge).
-- ============================================================

BEGIN TRANSACTION;

-- ────────────────────────────────────────────────────────────
-- STEP 1: Add the new column (if it doesn't already exist)
-- ────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('dbo.ChecklistItemTemplates')
      AND  name = 'EquipmentSubTypeId')
BEGIN
    ALTER TABLE [dbo].[ChecklistItemTemplates]
        ADD [EquipmentSubTypeId] INT NULL;
END;
GO

-- ────────────────────────────────────────────────────────────
-- STEP 2: Add FK + index (if not already present)
-- ────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE  name = 'FK_ChecklistItems_SubTypes'
      AND  parent_object_id = OBJECT_ID('dbo.ChecklistItemTemplates'))
BEGIN
    ALTER TABLE [dbo].[ChecklistItemTemplates]
        ADD CONSTRAINT [FK_ChecklistItems_SubTypes]
            FOREIGN KEY ([EquipmentSubTypeId])
            REFERENCES [dbo].[EquipmentSubTypes]([Id]);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.ChecklistItemTemplates')
      AND  name = 'IX_ChecklistItemTemplates_EquipmentSubTypeId')
    CREATE NONCLUSTERED INDEX [IX_ChecklistItemTemplates_EquipmentSubTypeId]
        ON [dbo].[ChecklistItemTemplates]([EquipmentSubTypeId]);
GO

-- ────────────────────────────────────────────────────────────
-- STEP 3: Insert CO2-specific checklist items.
--         We look up the sub-type IDs dynamically so this
--         script works regardless of what IDs were assigned.
-- ────────────────────────────────────────────────────────────

DECLARE @TypeId8   INT = 8;   -- Fire Extinguishers

-- Get CO2 sub-type IDs (inserted by FixLiveData.sql)
DECLARE @SubType5kgCO2 INT = (SELECT Id FROM [dbo].[EquipmentSubTypes] WHERE EquipmentTypeId = @TypeId8 AND Name = '5kg CO2');
DECLARE @SubType2kgCO2 INT = (SELECT Id FROM [dbo].[EquipmentSubTypes] WHERE EquipmentTypeId = @TypeId8 AND Name = '2kg CO2');

-- Only insert if the sub-types exist and no items exist for them yet
IF @SubType5kgCO2 IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM [dbo].[ChecklistItemTemplates]
    WHERE  EquipmentTypeId = @TypeId8 AND EquipmentSubTypeId = @SubType5kgCO2)
BEGIN
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the extinguisher accessible and not blocked?',                        1);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the discharge horn clean and unobstructed?',                          2);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the safety pin and tamper seal intact?',                             3);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the extinguisher free of dents, corrosion or damage?',               4);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the weight within the acceptable range? (CO2 has no pressure gauge)', 5);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the handle and trigger mechanism in good condition?',                  6);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the mounting bracket secure?',                                        7);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the inspection tag up to date?',                                      8);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType5kgCO2, 'Is the operating instructions label legible?',                           9);
END;

IF @SubType2kgCO2 IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM [dbo].[ChecklistItemTemplates]
    WHERE  EquipmentTypeId = @TypeId8 AND EquipmentSubTypeId = @SubType2kgCO2)
BEGIN
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the extinguisher accessible and not blocked?',                        1);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the discharge horn clean and unobstructed?',                          2);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the safety pin and tamper seal intact?',                             3);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the extinguisher free of dents, corrosion or damage?',               4);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the weight within the acceptable range? (CO2 has no pressure gauge)', 5);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the handle and trigger mechanism in good condition?',                  6);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the mounting bracket secure?',                                        7);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the inspection tag up to date?',                                      8);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [EquipmentSubTypeId], [ItemName], [SortOrder]) VALUES (@TypeId8, @SubType2kgCO2, 'Is the operating instructions label legible?',                           9);
END;

-- ────────────────────────────────────────────────────────────
-- VERIFICATION
-- ────────────────────────────────────────────────────────────

SELECT 'ChecklistTemplates' AS [Table],
       t.Id,
       et.Name  AS TypeName,
       st.Name  AS SubTypeName,
       t.ItemName,
       t.SortOrder
FROM   [dbo].[ChecklistItemTemplates] t
INNER JOIN [dbo].[EquipmentTypes]    et ON et.Id = t.EquipmentTypeId
LEFT  JOIN [dbo].[EquipmentSubTypes] st ON st.Id = t.EquipmentSubTypeId
WHERE  t.EquipmentTypeId = 8
ORDER  BY t.EquipmentSubTypeId, t.SortOrder;

-- ────────────────────────────────────────────────────────────
-- If results look correct → COMMIT, else → ROLLBACK
-- ────────────────────────────────────────────────────────────
-- COMMIT;
-- ROLLBACK;
