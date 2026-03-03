-- ============================================================
--  SafetyCompliance – Add Missing Performance Indexes
--  Run against your live database.
--  Safe to re-run: each CREATE INDEX is guarded by an
--  IF NOT EXISTS check on sys.indexes.
-- ============================================================

-- ── Plants ──────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Plants')
      AND  name = 'IX_Plants_CompanyId_IsActive')
    CREATE NONCLUSTERED INDEX [IX_Plants_CompanyId_IsActive]
        ON [dbo].[Plants]([CompanyId], [IsActive]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Plants')
      AND  name = 'IX_Plants_IsActive')
    CREATE NONCLUSTERED INDEX [IX_Plants_IsActive]
        ON [dbo].[Plants]([IsActive]) INCLUDE ([CompanyId]);
GO

-- ── Sections ────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Sections')
      AND  name = 'IX_Sections_PlantId')
    CREATE NONCLUSTERED INDEX [IX_Sections_PlantId]
        ON [dbo].[Sections]([PlantId]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Sections')
      AND  name = 'IX_Sections_PlantId_IsActive')
    CREATE NONCLUSTERED INDEX [IX_Sections_PlantId_IsActive]
        ON [dbo].[Sections]([PlantId], [IsActive]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Sections')
      AND  name = 'IX_Sections_IsActive')
    CREATE NONCLUSTERED INDEX [IX_Sections_IsActive]
        ON [dbo].[Sections]([IsActive]) INCLUDE ([PlantId]);
GO

-- ── Equipment ────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Equipment')
      AND  name = 'IX_Equipment_SectionId')
    CREATE NONCLUSTERED INDEX [IX_Equipment_SectionId]
        ON [dbo].[Equipment]([SectionId]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Equipment')
      AND  name = 'IX_Equipment_SectionId_IsActive')
    CREATE NONCLUSTERED INDEX [IX_Equipment_SectionId_IsActive]
        ON [dbo].[Equipment]([SectionId], [IsActive]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Equipment')
      AND  name = 'IX_Equipment_IsActive')
    CREATE NONCLUSTERED INDEX [IX_Equipment_IsActive]
        ON [dbo].[Equipment]([IsActive]) INCLUDE ([SectionId]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  object_id = OBJECT_ID('dbo.Equipment')
      AND  name = 'IX_Equipment_TypeId')
    CREATE NONCLUSTERED INDEX [IX_Equipment_TypeId]
        ON [dbo].[Equipment]([EquipmentTypeId]);
GO

PRINT 'All indexes created (or already existed).';
