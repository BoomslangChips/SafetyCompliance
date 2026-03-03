BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303081123_AddPlantContacts'
)
BEGIN
    CREATE TABLE [PlantContacts] (
        [Id] int NOT NULL IDENTITY,
        [PlantId] int NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Role] nvarchar(200) NULL,
        [Phone] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [Notes] nvarchar(1000) NULL,
        [IsPrimary] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedById] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedById] nvarchar(max) NULL,
        CONSTRAINT [PK_PlantContacts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlantContacts_Plants_PlantId] FOREIGN KEY ([PlantId]) REFERENCES [Plants] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303081123_AddPlantContacts'
)
BEGIN
    CREATE INDEX [IX_PlantContacts_PlantId] ON [PlantContacts] ([PlantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303081123_AddPlantContacts'
)
BEGIN
    CREATE INDEX [IX_PlantContacts_PlantId_Category] ON [PlantContacts] ([PlantId], [Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303081123_AddPlantContacts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260303081123_AddPlantContacts', N'10.0.3');
END;

COMMIT;
GO

