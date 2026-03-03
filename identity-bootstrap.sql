/*═══════════════════════════════════════════════════════════════════════════
  MIS — Majesty Oil Mills Inspection System
  IDENTITY TABLES BOOTSTRAP SCRIPT

  PURPOSE  : Creates all missing ASP.NET Identity tables, migration history,
             and related FK constraints so the application can start cleanly.
  IDEMPOTENT: Every block is guarded by IF NOT EXISTS — safe to re-run.
  HOW TO RUN: Open in SQL Server Management Studio, select your database,
              run the entire script (F5).

  Migrations recorded:
    • 20260227121125_AddPhotoSupport  (EF Core 10.0.3)
    • 20260302131640_AddNotes         (EF Core 10.0.3)
═══════════════════════════════════════════════════════════════════════════*/
SET NOCOUNT ON;

PRINT '── MIS Identity Bootstrap ──────────────────────────────────────────';
PRINT '';

/* ═══════════════════════════════════════════════════════════════════════
   1. __EFMigrationsHistory
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = '__EFMigrationsHistory' AND type = 'U')
BEGIN
    PRINT 'Creating [__EFMigrationsHistory] ...';
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId]    nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '  Done.';
END
ELSE
    PRINT '[__EFMigrationsHistory] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   2. AspNetRoles
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'AspNetRoles' AND type = 'U')
BEGIN
    PRINT 'Creating [AspNetRoles] ...';
    CREATE TABLE [AspNetRoles] (
        [Id]               nvarchar(450) NOT NULL,
        [Name]             nvarchar(256) NULL,
        [NormalizedName]   nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [RoleNameIndex]
        ON [AspNetRoles] ([NormalizedName])
        WHERE [NormalizedName] IS NOT NULL;
    PRINT '  Done.';
END
ELSE
    PRINT '[AspNetRoles] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   3. AspNetUsers  (includes custom ApplicationUser columns)
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'AspNetUsers' AND type = 'U')
BEGIN
    PRINT 'Creating [AspNetUsers] ...';
    CREATE TABLE [AspNetUsers] (
        [Id]                   nvarchar(450)  NOT NULL,
        -- Custom ApplicationUser properties
        [FirstName]            nvarchar(max)  NOT NULL DEFAULT N'',
        [LastName]             nvarchar(max)  NOT NULL DEFAULT N'',
        [IsActive]             bit            NOT NULL DEFAULT 1,
        -- Standard ASP.NET Identity columns
        [UserName]             nvarchar(256)  NULL,
        [NormalizedUserName]   nvarchar(256)  NULL,
        [Email]                nvarchar(256)  NULL,
        [NormalizedEmail]      nvarchar(256)  NULL,
        [EmailConfirmed]       bit            NOT NULL DEFAULT 0,
        [PasswordHash]         nvarchar(max)  NULL,
        [SecurityStamp]        nvarchar(max)  NULL,
        [ConcurrencyStamp]     nvarchar(max)  NULL,
        [PhoneNumber]          nvarchar(max)  NULL,
        [PhoneNumberConfirmed] bit            NOT NULL DEFAULT 0,
        [TwoFactorEnabled]     bit            NOT NULL DEFAULT 0,
        [LockoutEnd]           datetimeoffset NULL,
        [LockoutEnabled]       bit            NOT NULL DEFAULT 0,
        [AccessFailedCount]    int            NOT NULL DEFAULT 0,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    CREATE INDEX        [EmailIndex]
        ON [AspNetUsers] ([NormalizedEmail]);
    CREATE UNIQUE INDEX [UserNameIndex]
        ON [AspNetUsers] ([NormalizedUserName])
        WHERE [NormalizedUserName] IS NOT NULL;
    PRINT '  Done.';
END
ELSE
    PRINT '[AspNetUsers] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   4. AspNetRoleClaims
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'AspNetRoleClaims' AND type = 'U')
BEGIN
    PRINT 'Creating [AspNetRoleClaims] ...';
    CREATE TABLE [AspNetRoleClaims] (
        [Id]         int           NOT NULL IDENTITY(1,1),
        [RoleId]     nvarchar(450) NOT NULL,
        [ClaimType]  nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetRoleClaims_RoleId]
        ON [AspNetRoleClaims] ([RoleId]);
    PRINT '  Done.';
END
ELSE
    PRINT '[AspNetRoleClaims] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   5. AspNetUserClaims
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'AspNetUserClaims' AND type = 'U')
BEGIN
    PRINT 'Creating [AspNetUserClaims] ...';
    CREATE TABLE [AspNetUserClaims] (
        [Id]         int           NOT NULL IDENTITY(1,1),
        [UserId]     nvarchar(450) NOT NULL,
        [ClaimType]  nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserClaims_UserId]
        ON [AspNetUserClaims] ([UserId]);
    PRINT '  Done.';
END
ELSE
    PRINT '[AspNetUserClaims] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   6. AspNetUserLogins
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'AspNetUserLogins' AND type = 'U')
BEGIN
    PRINT 'Creating [AspNetUserLogins] ...';
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider]       nvarchar(450) NOT NULL,
        [ProviderKey]         nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId]              nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserLogins_UserId]
        ON [AspNetUserLogins] ([UserId]);
    PRINT '  Done.';
END
ELSE
    PRINT '[AspNetUserLogins] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   7. AspNetUserRoles
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'AspNetUserRoles' AND type = 'U')
BEGIN
    PRINT 'Creating [AspNetUserRoles] ...';
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserRoles_RoleId]
        ON [AspNetUserRoles] ([RoleId]);
    PRINT '  Done.';
END
ELSE
    PRINT '[AspNetUserRoles] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   8. AspNetUserTokens
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'AspNetUserTokens' AND type = 'U')
BEGIN
    PRINT 'Creating [AspNetUserTokens] ...';
    CREATE TABLE [AspNetUserTokens] (
        [UserId]        nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name]          nvarchar(450) NOT NULL,
        [Value]         nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT '  Done.';
END
ELSE
    PRINT '[AspNetUserTokens] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   9. UserCompanies — add FK to AspNetUsers (if missing)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'UserCompanies' AND type = 'U')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserCompanies_AspNetUsers_UserId')
    BEGIN
        PRINT 'Adding FK [UserCompanies] → [AspNetUsers] (UserId) ...';
        ALTER TABLE [UserCompanies]
            ADD CONSTRAINT [FK_UserCompanies_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
        PRINT '  Done.';
    END
    ELSE
        PRINT 'FK [FK_UserCompanies_AspNetUsers_UserId] already exists — skipped.';
END
ELSE
    PRINT '[UserCompanies] table not found — FK skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   10. InspectionRounds.Status — fix tinyint → int  (Migration 2 change)
       Also drops the legacy lookup FK if it exists.
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (
    SELECT 1
    FROM   sys.columns c
    JOIN   sys.tables  t ON c.object_id = t.object_id
    WHERE  t.name = 'InspectionRounds'
      AND  c.name = 'Status'
      AND  c.system_type_id = 48  -- 48 = tinyint
)
BEGIN
    PRINT 'Fixing [InspectionRounds].[Status] tinyint → int ...';

    -- Drop the legacy lookup FK to InspectionStatuses table if it exists
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Rounds_Status')
    BEGIN
        PRINT '  Dropping legacy constraint [FK_Rounds_Status] ...';
        ALTER TABLE [InspectionRounds] DROP CONSTRAINT [FK_Rounds_Status];
    END

    ALTER TABLE [InspectionRounds] ALTER COLUMN [Status] int NOT NULL;
    PRINT '  Done.';
END
ELSE
    PRINT '[InspectionRounds].[Status] is already int — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   11. InspectionRounds.InspectionScheduleId — add column if missing
       (Migration 2 added this column)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'InspectionRounds' AND type = 'U')
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM   sys.columns c
        JOIN   sys.tables  t ON c.object_id = t.object_id
        WHERE  t.name = 'InspectionRounds' AND c.name = 'InspectionScheduleId'
    )
    BEGIN
        PRINT 'Adding [InspectionRounds].[InspectionScheduleId] ...';
        ALTER TABLE [InspectionRounds] ADD [InspectionScheduleId] int NULL;
        PRINT '  Done.';
    END
    ELSE
        PRINT '[InspectionRounds].[InspectionScheduleId] already exists — skipped.';
END

/* ═══════════════════════════════════════════════════════════════════════
   12. InspectionRounds → InspectionSchedules FK (Migration 2)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'InspectionSchedules' AND type = 'U')
   AND EXISTS (
       SELECT 1 FROM sys.columns c
       JOIN sys.tables t ON c.object_id = t.object_id
       WHERE t.name = 'InspectionRounds' AND c.name = 'InspectionScheduleId'
   )
   AND NOT EXISTS (
       SELECT 1 FROM sys.foreign_keys
       WHERE  name = 'FK_InspectionRounds_InspectionSchedules_InspectionScheduleId'
   )
BEGIN
    PRINT 'Adding FK [InspectionRounds] → [InspectionSchedules] ...';
    ALTER TABLE [InspectionRounds]
        ADD CONSTRAINT [FK_InspectionRounds_InspectionSchedules_InspectionScheduleId]
        FOREIGN KEY ([InspectionScheduleId]) REFERENCES [InspectionSchedules] ([Id]);
    PRINT '  Done.';
END
ELSE IF EXISTS (
       SELECT 1 FROM sys.foreign_keys
       WHERE  name = 'FK_InspectionRounds_InspectionSchedules_InspectionScheduleId'
   )
    PRINT 'FK [FK_InspectionRounds_InspectionSchedules_InspectionScheduleId] already exists — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   13. InspectionRounds.IX_InspectionRounds_InspectionScheduleId
       (Migration 2 added this index)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'InspectionRounds' AND type = 'U')
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE  name = 'IX_InspectionRounds_InspectionScheduleId'
         AND  object_id = OBJECT_ID('InspectionRounds')
   )
   AND EXISTS (
       SELECT 1 FROM sys.columns c
       JOIN sys.tables t ON c.object_id = t.object_id
       WHERE t.name = 'InspectionRounds' AND c.name = 'InspectionScheduleId'
   )
BEGIN
    PRINT 'Adding index [IX_InspectionRounds_InspectionScheduleId] ...';
    CREATE INDEX [IX_InspectionRounds_InspectionScheduleId]
        ON [InspectionRounds] ([InspectionScheduleId]);
    PRINT '  Done.';
END

/* ═══════════════════════════════════════════════════════════════════════
   14. Remove stale IX_InspectionRounds_InspectedById /
       IX_InspectionRounds_ReviewedById (Migration 2 dropped these)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  name = 'IX_InspectionRounds_InspectedById'
      AND  object_id = OBJECT_ID('InspectionRounds')
)
BEGIN
    PRINT 'Dropping stale index [IX_InspectionRounds_InspectedById] ...';
    DROP INDEX [IX_InspectionRounds_InspectedById] ON [InspectionRounds];
    PRINT '  Done.';
END

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  name = 'IX_InspectionRounds_ReviewedById'
      AND  object_id = OBJECT_ID('InspectionRounds')
)
BEGIN
    PRINT 'Dropping stale index [IX_InspectionRounds_ReviewedById] ...';
    DROP INDEX [IX_InspectionRounds_ReviewedById] ON [InspectionRounds];
    PRINT '  Done.';
END

/* ═══════════════════════════════════════════════════════════════════════
   15. Drop legacy FK_InspectionRounds_AspNetUsers_InspectedById /
       FK_InspectionRounds_AspNetUsers_ReviewedById
       (Migration 2 dropped these — they'd conflict if they exist)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InspectionRounds_AspNetUsers_InspectedById')
BEGIN
    PRINT 'Dropping stale FK [FK_InspectionRounds_AspNetUsers_InspectedById] ...';
    ALTER TABLE [InspectionRounds] DROP CONSTRAINT [FK_InspectionRounds_AspNetUsers_InspectedById];
    PRINT '  Done.';
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InspectionRounds_AspNetUsers_ReviewedById')
BEGIN
    PRINT 'Dropping stale FK [FK_InspectionRounds_AspNetUsers_ReviewedById] ...';
    ALTER TABLE [InspectionRounds] DROP CONSTRAINT [FK_InspectionRounds_AspNetUsers_ReviewedById];
    PRINT '  Done.';
END

/* ═══════════════════════════════════════════════════════════════════════
   16. Drop stale IX_InspectionPhotos_UploadedById
       (Migration 2 dropped this index)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  name = 'IX_InspectionPhotos_UploadedById'
      AND  object_id = OBJECT_ID('InspectionPhotos')
)
BEGIN
    PRINT 'Dropping stale index [IX_InspectionPhotos_UploadedById] ...';
    DROP INDEX [IX_InspectionPhotos_UploadedById] ON [InspectionPhotos];
    PRINT '  Done.';
END

/* ═══════════════════════════════════════════════════════════════════════
   17. Drop stale FK_InspectionPhotos_AspNetUsers_UploadedById
       (Migration 2 dropped this FK)
   ═══════════════════════════════════════════════════════════════════════ */
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InspectionPhotos_AspNetUsers_UploadedById')
BEGIN
    PRINT 'Dropping stale FK [FK_InspectionPhotos_AspNetUsers_UploadedById] ...';
    ALTER TABLE [InspectionPhotos] DROP CONSTRAINT [FK_InspectionPhotos_AspNetUsers_UploadedById];
    PRINT '  Done.';
END

/* ═══════════════════════════════════════════════════════════════════════
   18. EquipmentInspections — drop Caption / Latitude / Longitude
       if they exist (Migration 2 dropped these columns from the
       InspectionPhotos / EquipmentInspections tables)
   ═══════════════════════════════════════════════════════════════════════ */
-- InspectionPhotos.Caption
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'InspectionPhotos' AND c.name = 'Caption'
)
BEGIN
    PRINT 'Dropping [InspectionPhotos].[Caption] (removed in Migration 2) ...';
    ALTER TABLE [InspectionPhotos] DROP COLUMN [Caption];
    PRINT '  Done.';
END

-- EquipmentInspections.Latitude
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'EquipmentInspections' AND c.name = 'Latitude'
)
BEGIN
    PRINT 'Dropping [EquipmentInspections].[Latitude] (removed in Migration 2) ...';
    ALTER TABLE [EquipmentInspections] DROP COLUMN [Latitude];
    PRINT '  Done.';
END

-- EquipmentInspections.Longitude
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'EquipmentInspections' AND c.name = 'Longitude'
)
BEGIN
    PRINT 'Dropping [EquipmentInspections].[Longitude] (removed in Migration 2) ...';
    ALTER TABLE [EquipmentInspections] DROP COLUMN [Longitude];
    PRINT '  Done.';
END

/* ═══════════════════════════════════════════════════════════════════════
   19. Record both EF migrations as applied
   ═══════════════════════════════════════════════════════════════════════ */
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE  [MigrationId] = N'20260227121125_AddPhotoSupport'
)
BEGIN
    PRINT 'Recording migration [20260227121125_AddPhotoSupport] ...';
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260227121125_AddPhotoSupport', N'10.0.3');
    PRINT '  Done.';
END
ELSE
    PRINT 'Migration [20260227121125_AddPhotoSupport] already recorded — skipped.';

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE  [MigrationId] = N'20260302131640_AddNotes'
)
BEGIN
    PRINT 'Recording migration [20260302131640_AddNotes] ...';
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260302131640_AddNotes', N'10.0.3');
    PRINT '  Done.';
END
ELSE
    PRINT 'Migration [20260302131640_AddNotes] already recorded — skipped.';

/* ═══════════════════════════════════════════════════════════════════════
   20. (OPTIONAL) Seed first admin user
       Uncomment, set your own PasswordHash, and run once.
       Generate a valid ASP.NET Identity password hash via:
         dotnet run --project src/SafetyCompliance.Web -- seed-admin
       OR use an online BCrypt tool and format it as an Identity v3 hash.
   ═══════════════════════════════════════════════════════════════════════ */
-- IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [NormalizedUserName] = N'ADMIN')
-- BEGIN
--     DECLARE @newId nvarchar(450) = LOWER(NEWID());
--     INSERT INTO [AspNetUsers] (
--         [Id], [FirstName], [LastName], [IsActive],
--         [UserName], [NormalizedUserName],
--         [Email],    [NormalizedEmail],  [EmailConfirmed],
--         [PasswordHash], [SecurityStamp], [ConcurrencyStamp],
--         [LockoutEnabled], [AccessFailedCount],
--         [PhoneNumberConfirmed], [TwoFactorEnabled]
--     ) VALUES (
--         @newId, N'Admin', N'User', 1,
--         N'admin', N'ADMIN',
--         N'admin@mis.local', N'ADMIN@MIS.LOCAL', 1,
--         N'<PASTE_ASPNET_IDENTITY_V3_HASH_HERE>',
--         LOWER(NEWID()), LOWER(NEWID()),
--         0, 0, 0, 0
--     );
--     PRINT 'Admin user seeded.';
-- END

PRINT '';
PRINT '── Bootstrap complete ──────────────────────────────────────────────';
PRINT 'All Identity tables created and migrations recorded.';
PRINT 'You can now start the application — the AspNetUsers error is resolved.';
PRINT '';
PRINT 'Next step: Create your first user via the application registration page';
PRINT 'or uncomment and run block 20 above to seed an admin user.';
