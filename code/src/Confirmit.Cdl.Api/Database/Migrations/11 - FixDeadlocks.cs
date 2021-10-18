using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(11)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class FixDeadlocks : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- Remove foreign keys
ALTER TABLE AccessedDocuments DROP CONSTRAINT FK_AccessedDocuments_Documents
GO
ALTER TABLE Commits DROP CONSTRAINT FK_Commits_Documents
GO
ALTER TABLE Commits DROP CONSTRAINT [FK_Commits_Revisions]
GO
ALTER TABLE Revisions DROP CONSTRAINT FK_Revision_Document
GO

-- Create copy of table Documents
CREATE TABLE [dbo].[DocumentsTmp](
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
 CONSTRAINT [PK_dbo.DocumentsTmp] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Copy data
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
	[PrivateMetadata],
	[HubId],
	[LinkedSurveyId])
SELECT
	Documents.[Id],
	Documents.[Name],
	Documents.[Created],
	Documents.[CreatedBy],
	Documents.[CreatedByName],
	Documents.[Modified],
	Documents.[ModifiedBy],
	Documents.[ModifiedByName],
	Documents.[CompanyId],
	Documents.[CompanyName],
	Documents.[SourceCode],
	Documents.[SourceCodeEditOps],
	Documents.[PublishedRevisionId],
	Documents.[PublicMetadata],
	Documents.[PrivateMetadata],
	Documents.[HubId],
	Documents.[LinkedSurveyId]
FROM Documents

SET IDENTITY_INSERT DocumentsTmp OFF
GO

-- Remove old table
DROP TABLE Documents
GO

-- Rename new table
EXEC sp_rename 'DocumentsTmp', 'Documents'

-- Create foreign keys
ALTER TABLE [dbo].[Commits]  WITH CHECK ADD  CONSTRAINT [FK_Commits_Documents] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Documents] ([Id])
ON DELETE CASCADE
GO
 
ALTER TABLE [dbo].[Commits] CHECK CONSTRAINT [FK_Commits_Documents]
GO

 
ALTER TABLE [dbo].[Commits]  WITH CHECK ADD  CONSTRAINT [FK_Commits_Revisions] FOREIGN KEY([RevisionId])
REFERENCES [dbo].[Revisions] ([Id])
GO
 
ALTER TABLE [dbo].[Commits] CHECK CONSTRAINT [FK_Commits_Revisions]
GO


ALTER TABLE [dbo].[AccessedDocuments]  WITH CHECK ADD  CONSTRAINT [FK_AccessedDocuments_Documents] FOREIGN KEY([Id])
REFERENCES [dbo].[Documents] ([Id])
ON DELETE CASCADE
GO
 
ALTER TABLE [dbo].[AccessedDocuments] CHECK CONSTRAINT [FK_AccessedDocuments_Documents]
GO


ALTER TABLE Revisions  WITH CHECK ADD  CONSTRAINT FK_Revision_Document FOREIGN KEY (DocumentId)
REFERENCES Documents (Id)
ON DELETE CASCADE
GO

ALTER TABLE Revisions CHECK CONSTRAINT FK_Revision_Document
GO


CREATE TRIGGER [dbo].[TR_Document_Delete] ON [dbo].[Documents]
FOR DELETE
AS
BEGIN
	SET NOCOUNT ON

	DELETE FROM UserPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

	DELETE FROM EnduserPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

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