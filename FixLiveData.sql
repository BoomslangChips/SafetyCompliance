-- ============================================================
--  SafetyCompliance – Live Data Fix Script
--  Run against your production / live database.
--  Safe to re-run: uses IF NOT EXISTS / existence checks.
-- ============================================================

BEGIN TRANSACTION;

-- ────────────────────────────────────────────────────────────
-- STEP 1: Safely remove the duplicate "Fire Extinguishers"
--         type (Id = 9, Description = '5Kg Co2').
--         First re-home any equipment pointing at it (unlikely,
--         but safe), then delete its checklist templates, then
--         delete the type row itself.
-- ────────────────────────────────────────────────────────────

-- 1a. Move any equipment that somehow uses TypeId=9 → TypeId=8
UPDATE [dbo].[Equipment]
SET    [EquipmentTypeId] = 8
WHERE  [EquipmentTypeId] = 9;

-- 1b. Delete checklist responses linked to templates for TypeId=9
DELETE r
FROM   [dbo].[EquipmentInspectionChecklistResponses] r
INNER JOIN [dbo].[ChecklistItemTemplates] t ON r.[ChecklistItemTemplateId] = t.[Id]
WHERE  t.[EquipmentTypeId] = 9;

-- 1c. Delete checklist templates for TypeId=9
DELETE FROM [dbo].[ChecklistItemTemplates]
WHERE  [EquipmentTypeId] = 9;

-- 1d. Delete the duplicate type
DELETE FROM [dbo].[EquipmentTypes]
WHERE  [Id] = 9;

-- ────────────────────────────────────────────────────────────
-- STEP 2: Tidy up EquipmentTypes for all your types.
--         - TypeId=8: Strip the hard-coded "9Kg DCP" from the
--           description (size/agent now lives in sub-types).
--         - TypeId=6 / 7 / 10: Set sensible descriptions.
-- ────────────────────────────────────────────────────────────

UPDATE [dbo].[EquipmentTypes]
SET    [Description] = 'Portable fire suppression device'
WHERE  [Id] = 8;

UPDATE [dbo].[EquipmentTypes]
SET    [Description] = 'Wall-mounted hose reel unit'
WHERE  [Id] = 6;

UPDATE [dbo].[EquipmentTypes]
SET    [Description] = 'Lay-flat hose in a wall-mounted box'
WHERE  [Id] = 7;

UPDATE [dbo].[EquipmentTypes]
SET    [Description] = 'Manual / automatic fire alarm station'
WHERE  [Id] = 10;

-- ────────────────────────────────────────────────────────────
-- STEP 3: Add sub-types for Fire Extinguishers (TypeId = 8).
--         Only inserts rows that don't already exist.
-- ────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '9kg DCP')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '9kg DCP');

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '4.5kg DCP')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '4.5kg DCP');

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '2.5kg DCP')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '2.5kg DCP');

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '1.5kg DCP')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '1.5kg DCP');

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '5kg CO2')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '5kg CO2');

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '2kg CO2')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '2kg CO2');

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '9L Foam')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '9L Foam');

IF NOT EXISTS (SELECT 1 FROM [dbo].[EquipmentSubTypes] WHERE [EquipmentTypeId] = 8 AND [Name] = '9L Water')
    INSERT INTO [dbo].[EquipmentSubTypes] ([EquipmentTypeId], [Name]) VALUES (8, '9L Water');

-- ────────────────────────────────────────────────────────────
-- STEP 4: Link existing equipment to their sub-types and
--         normalise the Size field.
--
--  Current data:  TypeId=8, Size='9Kg', Description='DCP'
--  Target:        EquipmentSubTypeId = (id of '9kg DCP'), Size='9kg DCP'
-- ────────────────────────────────────────────────────────────

-- 4a. DCP extinguishers that had Size '9Kg' or '9kg'
UPDATE e
SET    e.[EquipmentSubTypeId] = st.[Id],
       e.[Size]               = st.[Name]
FROM   [dbo].[Equipment] e
INNER JOIN [dbo].[EquipmentSubTypes] st
    ON  st.[EquipmentTypeId] = 8
    AND st.[Name] = '9kg DCP'
WHERE  e.[EquipmentTypeId] = 8
  AND  (LOWER(e.[Size]) LIKE '9k%' OR e.[Description] LIKE '%DCP%')
  AND  e.[EquipmentSubTypeId] IS NULL;

-- 4b. CO2 extinguishers (5kg) – handles if any were re-homed from TypeId=9
UPDATE e
SET    e.[EquipmentSubTypeId] = st.[Id],
       e.[Size]               = st.[Name]
FROM   [dbo].[Equipment] e
INNER JOIN [dbo].[EquipmentSubTypes] st
    ON  st.[EquipmentTypeId] = 8
    AND st.[Name] = '5kg CO2'
WHERE  e.[EquipmentTypeId] = 8
  AND  (LOWER(e.[Size]) LIKE '5k%' OR e.[Description] LIKE '%CO2%' OR e.[Description] LIKE '%Co2%')
  AND  e.[EquipmentSubTypeId] IS NULL;

-- ────────────────────────────────────────────────────────────
-- STEP 5: Add checklist item templates for your equipment types.
--         Only inserts rows that don't already exist.
-- ────────────────────────────────────────────────────────────

-- ── 5a. Hose Reel (TypeId = 6) ──────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ChecklistItemTemplates] WHERE [EquipmentTypeId] = 6)
BEGIN
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (6, 'Is the hose reel accessible and signage visible?', 1);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (6, 'Is the hose in good condition (no cracks, splits or perishing)?', 2);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (6, 'Does the nozzle operate correctly?', 3);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (6, 'Is the shut-off valve fully operational with no leaks?', 4);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (6, 'Does the hose reel drum rotate freely?', 5);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (6, 'Is the water flow adequate when tested?', 6);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (6, 'Is the inspection/service tag current?', 7);
END;

-- ── 5b. Lay Flat Hose Boxes (TypeId = 7) ────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ChecklistItemTemplates] WHERE [EquipmentTypeId] = 7)
BEGIN
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (7, 'Is the hose box accessible and clearly marked?', 1);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (7, 'Is the lay-flat hose free of damage, kinks or deterioration?', 2);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (7, 'Are all couplings and fittings secure and undamaged?', 3);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (7, 'Is the nozzle/branch present and in working order?', 4);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (7, 'Is the hose box door/cover functional and not obstructed?', 5);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (7, 'Is the inspection/service tag current?', 6);
END;

-- ── 5c. Fire Extinguishers (TypeId = 8) ─────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ChecklistItemTemplates] WHERE [EquipmentTypeId] = 8)
BEGIN
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the extinguisher accessible and not blocked?', 1);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the pressure gauge in the green zone?', 2);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the safety pin and tamper seal intact?', 3);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the extinguisher clean and free of damage?', 4);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the nozzle/hose in good condition?', 5);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the mounting bracket secure?', 6);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the inspection tag up to date?', 7);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (8, 'Is the operating instructions label legible?', 8);
END;

-- ── 5d. Alarm Station (TypeId = 10) ─────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ChecklistItemTemplates] WHERE [EquipmentTypeId] = 10)
BEGIN
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (10, 'Is the alarm panel showing normal / clear status?', 1);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (10, 'Are all manual call points accessible and unobstructed?', 2);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (10, 'Is the sounder/strobe test successful?', 3);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (10, 'Is the manual call point glass/element intact?', 4);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (10, 'Is the alarm station signage clearly visible?', 5);
    INSERT INTO [dbo].[ChecklistItemTemplates] ([EquipmentTypeId], [ItemName], [SortOrder]) VALUES (10, 'Is the last service/test date within the required interval?', 6);
END;

-- ────────────────────────────────────────────────────────────
-- VERIFICATION – review counts before committing
-- ────────────────────────────────────────────────────────────
SELECT 'EquipmentTypes' AS [Table], Id, Name, Description
FROM   [dbo].[EquipmentTypes]
ORDER  BY Id;

SELECT 'SubTypes' AS [Table], st.Id, st.Name, et.Name AS TypeName
FROM   [dbo].[EquipmentSubTypes] st
INNER JOIN [dbo].[EquipmentTypes] et ON et.Id = st.EquipmentTypeId
ORDER  BY et.Id, st.Id;

SELECT 'Equipment' AS [Table], e.Id, e.Identifier, et.Name AS TypeName,
       st.Name AS SubTypeName, e.Size, e.Description
FROM   [dbo].[Equipment] e
INNER JOIN [dbo].[EquipmentTypes] et ON et.Id = e.EquipmentTypeId
LEFT  JOIN [dbo].[EquipmentSubTypes] st ON st.Id = e.EquipmentSubTypeId
ORDER  BY e.Id;

SELECT 'ChecklistTemplates' AS [Table], t.Id, et.Name AS TypeName, t.ItemName, t.SortOrder
FROM   [dbo].[ChecklistItemTemplates] t
INNER JOIN [dbo].[EquipmentTypes] et ON et.Id = t.EquipmentTypeId
ORDER  BY et.Id, t.SortOrder;

-- ────────────────────────────────────────────────────────────
-- If the verification results look correct, run:  COMMIT;
-- If anything looks wrong, run:                   ROLLBACK;
-- ────────────────────────────────────────────────────────────
-- COMMIT;
-- ROLLBACK;
