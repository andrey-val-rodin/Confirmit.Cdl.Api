using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(7)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class RedesignDatabase : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- *** Change table Revisions ***
-- Rename columns
EXEC sp_RENAME 'Revisions.RevisionNum' , 'Id', 'COLUMN'
EXEC sp_RENAME 'Revisions.DocumentName' , 'Name', 'COLUMN'
EXEC sp_RENAME 'Revisions.Timestamp' , 'Created', 'COLUMN'
EXEC sp_RENAME 'Revisions.ModifiedBy' , 'CreatedBy', 'COLUMN'
EXEC sp_RENAME 'Revisions.ModifiedByName' , 'CreatedByName', 'COLUMN'
GO

-- *** Change table Documents ***
-- Add columns Modified, ModifiedBy and ModifiedByName
ALTER TABLE Documents ADD
	ModifiedBy [int] NULL,
	ModifiedByName [nvarchar](256) NULL,
	Modified [datetime] NULL
GO

-- Copy data from drafts table
UPDATE Documents SET
	ModifiedBy = ISNULL((SELECT ModifiedBy FROM Drafts WHERE DocumentId = Documents.Id), Documents.CreatedBy),
	ModifiedByName = ISNULL((SELECT ModifiedByName FROM Drafts WHERE DocumentId = Documents.Id), Documents.CreatedByName),
	Modified = ISNULL((SELECT [Timestamp] FROM Drafts WHERE DocumentId = Documents.Id), Documents.Created)
GO

-- Set columns Modified, ModifiedBy and ModifiedByName NOT NULL
ALTER TABLE Documents ALTER COLUMN ModifiedBy [int] NULL
GO

ALTER TABLE Documents ALTER COLUMN ModifiedByName [nvarchar](256) NULL
GO

ALTER TABLE Documents ALTER COLUMN Modified [datetime] NULL
GO

-- Change CompanyId column (NULL => NOT NULL)
-- Add temporary column
ALTER TABLE Documents ADD
	CompanyIdTmp int NOT NULL Default 0
GO

-- Copy data
UPDATE Documents SET
	CompanyIdTmp = ISNULL(CompanyId, 0)

-- Delete column CompanyId
ALTER TABLE Documents
	DROP COLUMN CompanyId

-- Rename column CompanyIdTmp
EXEC sp_RENAME 'Documents.CompanyIdTmp' , 'CompanyId', 'COLUMN'
GO

-- Add columns SourceCode, SourceCodeEditOps, PublishedRevisionId, PublicMetadata and PrivateMetadata
ALTER TABLE Documents ADD
	SourceCode [nvarchar](MAX),
	SourceCodeEditOps [nvarchar](MAX),
	PublishedRevisionId [bigint] NULL,
	PublicMetadata [nvarchar](512) NULL,
	PrivateMetadata [nvarchar](512) NULL
GO

-- Copy data from drafts table
UPDATE Documents SET
	SourceCode = (SELECT SourceCode FROM Drafts WHERE DocumentId = Documents.Id),
	SourceCodeEditOps = (SELECT SourceCodeEditOps FROM Drafts WHERE DocumentId = Documents.Id),
	PublicMetadata = (SELECT PublicMetadata FROM Drafts WHERE DocumentId = Documents.Id),
	PrivateMetadata = (SELECT PrivateMetadata FROM Drafts WHERE DocumentId = Documents.Id)
GO

-- Set correct PublishedRevisionId values
UPDATE Documents SET
	PublishedRevisionId = (SELECT TOP 1 Id FROM Revisions WHERE DocumentId = Documents.Id ORDER BY Created DESC)
GO

-- Change trigger
ALTER TRIGGER TR_Document_Delete ON Documents
INSTEAD OF DELETE
AS
BEGIN
	SET NOCOUNT ON

	DELETE FROM UserPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

	DELETE FROM EnduserPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

	DELETE FROM Revisions
		WHERE DocumentId IN (SELECT Id FROM deleted)

	-- Now delete rows themselves
	DELETE FROM Documents
		WHERE Id IN (SELECT Id FROM deleted)
END
GO

-- *** Remove tables Drafts, RevisionTags and Tags
DROP TABLE Drafts
GO

DROP TABLE RevisionTags
GO

DROP TABLE Tags
GO

");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}