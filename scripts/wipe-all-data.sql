-- ============================================================
-- WIPE ALL DATA — SafetyCompliance Database
-- ============================================================
-- This script removes ALL data from every table in the database.
-- Foreign key constraints are temporarily disabled so that
-- tables can be deleted in any order, then re-enabled.
--
-- WARNING: This is IRREVERSIBLE. Back up your database first
--          if you need to preserve anything.
-- ============================================================

-- Disable all foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- ============================================================
-- 1. Application Domain Tables
-- ============================================================

-- Child / junction tables first (for clarity, though FKs are off)
DELETE FROM [dbo].[InspectionPhotos];
DELETE FROM [dbo].[InspectionResponses];
DELETE FROM [dbo].[Comments];
DELETE FROM [dbo].[Notes];
DELETE FROM [dbo].[Issues];
DELETE FROM [dbo].[ServiceBookings];
DELETE FROM [dbo].[ChecklistItemTemplates];
DELETE FROM [dbo].[EquipmentInspections];
DELETE FROM [dbo].[InspectionRounds];
DELETE FROM [dbo].[InspectionSchedules];
DELETE FROM [dbo].[Equipment];
DELETE FROM [dbo].[EquipmentSubTypes];
DELETE FROM [dbo].[EquipmentTypes];
DELETE FROM [dbo].[PlantContacts];
DELETE FROM [dbo].[Sections];
DELETE FROM [dbo].[Plants];
DELETE FROM [dbo].[UserCompanies];
DELETE FROM [dbo].[Companies];

-- ============================================================
-- 2. ASP.NET Identity Tables
-- ============================================================

DELETE FROM [dbo].[AspNetUserTokens];
DELETE FROM [dbo].[AspNetUserLogins];
DELETE FROM [dbo].[AspNetUserClaims];
DELETE FROM [dbo].[AspNetRoleClaims];
DELETE FROM [dbo].[AspNetUserRoles];
DELETE FROM [dbo].[AspNetUsers];
DELETE FROM [dbo].[AspNetRoles];

-- ============================================================
-- 3. Entity Framework Migrations History (optional)
--    Uncomment the line below if you also want to clear
--    migration history (you'll need to re-run migrations).
-- ============================================================
-- DELETE FROM [dbo].[__EFMigrationsHistory];

-- ============================================================
-- 4. Tables that may exist if migrations were run recently
-- ============================================================

-- These tables are defined in DbContext but may not exist yet.
-- The IF EXISTS guard prevents errors if they haven't been created.
IF OBJECT_ID('[dbo].[EquipmentCheckRecords]', 'U') IS NOT NULL
    DELETE FROM [dbo].[EquipmentCheckRecords];

IF OBJECT_ID('[dbo].[EquipmentChecks]', 'U') IS NOT NULL
    DELETE FROM [dbo].[EquipmentChecks];

-- ============================================================
-- Re-enable all foreign key constraints
-- ============================================================
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

-- ============================================================
-- Reset all identity columns back to 1
-- ============================================================
EXEC sp_MSforeachtable '
    IF OBJECTPROPERTY(OBJECT_ID(''?''), ''TableHasIdentity'') = 1
        DBCC CHECKIDENT (''?'', RESEED, 0)
';

PRINT '=== All data has been removed. Database is clean. ===';
