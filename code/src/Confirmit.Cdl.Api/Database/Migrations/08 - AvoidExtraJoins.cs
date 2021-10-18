using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(8)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class AvoidExtraJoins : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- Remove foreign key
ALTER TABLE Revisions DROP CONSTRAINT FK_Revision_Document
GO

-- Create copy of table Documents
CREATE TABLE DocumentsTmp(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedByName] [nvarchar](256) NOT NULL,
	[Modified] [datetime] NOT NULL,
	[ModifiedBy] [int] NOT NULL,
	[ModifiedByName] [nvarchar](256) NOT NULL,
	[CompanyId] [int] NOT NULL,
	[CompanyName] [nvarchar](64) NOT NULL,
	[SourceCode] [nvarchar](max) NOT NULL,
	[SourceCodeEditOps] [nvarchar](max) NULL,
	[PublishedRevisionId] [bigint] NULL,
	[PublicMetadata] [nvarchar](512) NULL,
	[PrivateMetadata] [nvarchar](512) NULL,
	[HubId] [bigint] NULL,
	[LinkedSurveyId] [nvarchar](50) NULL,
 CONSTRAINT [PK_dbo.DocumentsTmp] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Copy rows
SET IDENTITY_INSERT DocumentsTmp ON
INSERT INTO DocumentsTmp (
	[Id],
	[Name],
	[Created],
	[CreatedBy],
	[CreatedByName],
	[Modified],
	[ModifiedBy],
	[ModifiedByName],
	[CompanyId],
	[CompanyName],
	[SourceCode],
	[SourceCodeEditOps],
	[PublishedRevisionId],
	[PublicMetadata],
	[PrivateMetadata])
SELECT
	Documents.[Id],
	Documents.[Name],
	Documents.[Created],
	Documents.[CreatedBy],
	ISNULL(Creators.FirstName + ' ' + Creators.LastName, ''),
	Documents.[Modified],
	Documents.[ModifiedBy],
	ISNULL(Modifiers.FirstName + ' ' + Modifiers.LastName, ''),
	Documents.[CompanyId],
	ISNULL(Companies.[Name], ''),
	ISNULL([SourceCode], ''),
	Documents.[SourceCodeEditOps],
	Documents.[PublishedRevisionId],
	Documents.[PublicMetadata],
	Documents.[PrivateMetadata]
FROM Documents
LEFT OUTER JOIN Users Creators ON Documents.CreatedBy = Creators.Id
LEFT OUTER JOIN Users Modifiers ON Documents.CreatedBy = Modifiers.Id
LEFT OUTER JOIN Companies ON Documents.CompanyId = Companies.Id

SET IDENTITY_INSERT DocumentsTmp OFF
GO

-- Create copy of table Revisions
CREATE TABLE RevisionsTmp(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentId] [bigint] NOT NULL,
	[Number] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedByName] [nvarchar](256) NOT NULL,
	[SourceCode] [nvarchar](max) NOT NULL,
	[PublicMetadata] [nvarchar](512) NULL,
	[PrivateMetadata] [nvarchar](512) NULL,
	[HubId] [bigint] NULL,
	[LinkedSurveyId] [nvarchar](50) NULL,
 CONSTRAINT [PK_dbo.RevisionsTmp] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Copy rows
SET IDENTITY_INSERT RevisionsTmp ON
INSERT INTO RevisionsTmp (
    [Id],
	[DocumentId],
	[Number],
	[Name],
	[Created],
	[CreatedBy],
	[CreatedByName],
	[SourceCode],
	[PublicMetadata],
	[PrivateMetadata])
SELECT
    Revisions.[Id],
	Revisions.[DocumentId],
	0,
	Revisions.[Name],
	Revisions.[Created],
	Revisions.[CreatedBy],
	ISNULL(Creators.FirstName + ' ' + Creators.LastName, ''),
	Revisions.[SourceCode],
	Revisions.[PublicMetadata],
	Revisions.[PrivateMetadata]
FROM Revisions
LEFT OUTER JOIN Users Creators ON Revisions.CreatedBy = Creators.Id

SET IDENTITY_INSERT RevisionsTmp OFF
GO

-- Remove old tables and keys
DROP TABLE Documents
DROP TABLE Revisions
GO

-- Rename new tables and keys
EXEC sp_rename 'DocumentsTmp', 'Documents'
EXEC sp_rename '[dbo].[Documents].[PK_dbo.DocumentsTmp]', 'PK_dbo.Documents'
EXEC sp_rename 'RevisionsTmp', 'Revisions'
EXEC sp_rename '[dbo].[Revisions].[PK_dbo.RevisionsTmp]', 'PK_dbo.Revisions'
GO

-- Add index by DocumentId
CREATE NONCLUSTERED INDEX [IX_RevisionDocumentId] ON [dbo].[Revisions]
(
	[DocumentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

-- Restore foreign key
ALTER TABLE Revisions  WITH CHECK ADD  CONSTRAINT FK_Revision_Document FOREIGN KEY (DocumentId)
REFERENCES Documents (Id)
GO

ALTER TABLE Revisions CHECK CONSTRAINT FK_Revision_Document
GO

CREATE TRIGGER [dbo].[TR_Document_Delete] ON [dbo].[Documents]
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

CREATE TRIGGER [dbo].[TR_Document_Insert] 
   ON  [dbo].[Documents]
   FOR INSERT 
AS 
BEGIN
	SET NOCOUNT ON;

	INSERT INTO UserPermissions(ObjectId, ObjectType, AccountId, AccountType, Permission)
	SELECT
		inserted.Id,		-- Document Id
		1,					-- Document type
		inserted.CreatedBy, -- User Id
		1,					-- User type
		2					-- Permission.Manage
	FROM inserted
END
GO

ALTER TABLE [dbo].[Documents] ENABLE TRIGGER [TR_Document_Delete]
ALTER TABLE [dbo].[Documents] ENABLE TRIGGER [TR_Document_Insert]
GO
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}