-- Migration: AddContactDocuments
-- Creates the ContactDocuments table for storing file attachments on plant contacts.
-- Run this against your database before using the new document attachment features.

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactDocuments')
BEGIN
    CREATE TABLE [ContactDocuments] (
        [Id]              INT            IDENTITY(1,1) NOT NULL,
        [PlantContactId]  INT            NOT NULL,
        [FileName]        NVARCHAR(500)  NOT NULL,
        [DisplayName]     NVARCHAR(500)  NOT NULL,
        [ContentType]     NVARCHAR(200)  NOT NULL,
        [FileSizeBytes]   BIGINT         NOT NULL,
        [FileBase64]      NVARCHAR(MAX)  NOT NULL,
        [CreatedAt]       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [CreatedById]     NVARCHAR(450)  NOT NULL DEFAULT '',
        [ModifiedAt]      DATETIME2      NULL,
        [ModifiedById]    NVARCHAR(450)  NULL,
        CONSTRAINT [PK_ContactDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContactDocuments_PlantContacts_PlantContactId]
            FOREIGN KEY ([PlantContactId])
            REFERENCES [PlantContacts] ([Id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_ContactDocuments_PlantContactId]
        ON [ContactDocuments] ([PlantContactId]);

    PRINT 'ContactDocuments table created successfully.';
END
ELSE
BEGIN
    PRINT 'ContactDocuments table already exists — skipping.';
END
