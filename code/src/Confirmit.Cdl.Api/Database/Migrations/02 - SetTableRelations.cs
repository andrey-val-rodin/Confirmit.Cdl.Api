using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(2)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class SetTableRelations : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- Delete all drafts for non-existent documents
DELETE FROM [dbo].[Drafts]
	wHERE DocumentId NOT IN (SELECT id FROM [dbo].[Documents])

-- Remove cascade deletion from FK_Revision_Document constraint
IF OBJECT_ID('[dbo].[FK_Revision_Document]') IS NOT NULL
	ALTER TABLE [dbo].[Revisions] DROP CONSTRAINT [FK_Revision_Document]
GO

ALTER TABLE [dbo].[Revisions]  WITH CHECK ADD  CONSTRAINT [FK_Revision_Document] FOREIGN KEY([DocumentId])
	REFERENCES [dbo].[Documents] ([Id])
GO

ALTER TABLE [dbo].[Revisions] CHECK CONSTRAINT [FK_Revision_Document]
GO

-- Append primary key to Drafts table
IF OBJECT_ID('[dbo].[PK_Drafts]') IS NOT NULL
	ALTER TABLE [dbo].[Drafts] DROP CONSTRAINT [PK_Drafts]
GO

ALTER TABLE [dbo].[Drafts] ADD  CONSTRAINT [PK_Drafts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

-- Append foreign constraint to Drafts table
IF OBJECT_ID('[dbo].[FK_Draft_Document]') IS NOT NULL
	ALTER TABLE [dbo].[Drafts] DROP CONSTRAINT [FK_Draft_Document]
GO

ALTER TABLE [dbo].[Drafts]  WITH CHECK ADD  CONSTRAINT [FK_Draft_Document] FOREIGN KEY([DocumentId])
	REFERENCES [dbo].[Documents] ([Id])
GO

ALTER TABLE [dbo].[Drafts] CHECK CONSTRAINT [FK_Draft_Document]
GO

-- Replace constraint FK_RevisionTags_Document on FK_RevisionTag_Document, which doesn't contain cascade deletion
IF OBJECT_ID('[dbo].[FK_RevisionTags_Document]') IS NOT NULL
	ALTER TABLE [dbo].[RevisionTags] DROP CONSTRAINT [FK_RevisionTags_Document]
GO
IF OBJECT_ID('[dbo].[FK_RevisionTag_Document]') IS NOT NULL
	ALTER TABLE [dbo].[RevisionTags] DROP CONSTRAINT [FK_RevisionTag_Document]
GO

ALTER TABLE [dbo].[RevisionTags]  WITH CHECK ADD  CONSTRAINT [FK_RevisionTag_Document] FOREIGN KEY([DocumentId])
	REFERENCES [dbo].[Documents] ([Id])
GO

ALTER TABLE [dbo].[RevisionTags] CHECK CONSTRAINT [FK_RevisionTag_Document]
GO

-- Append foreign constraint FK_RevisionTag_Tag to RevisionTags table
IF OBJECT_ID('[dbo].[FK_RevisionTag_Tag]') IS NOT NULL
	ALTER TABLE [dbo].[RevisionTags] DROP CONSTRAINT [FK_RevisionTag_Tag]
GO

ALTER TABLE [dbo].[RevisionTags]  WITH CHECK ADD  CONSTRAINT [FK_RevisionTag_Tag] FOREIGN KEY([TagId])
	REFERENCES [dbo].[Tags] ([Id])
GO

ALTER TABLE [dbo].[RevisionTags] CHECK CONSTRAINT [FK_RevisionTag_Tag]
GO

-- Finally, modify DELETE trigger
IF EXISTS (SELECT null FROM sysobjects WHERE type = 'tr' and name = 'TR_Document_Delete')
	DROP TRIGGER [dbo].[TR_Document_Delete]
GO

CREATE TRIGGER [dbo].[TR_Document_Delete] on [dbo].[Documents]
INSTEAD OF DELETE
AS
BEGIN
	SET NOCOUNT ON

	DELETE FROM UserPermissions
		WHERE ObjectId IN (SELECT ID FROM deleted) AND ObjectType = 1

	DELETE FROM EnduserPermissions
		WHERE ObjectId IN (SELECT ID FROM deleted) AND ObjectType = 1

	DELETE FROM Revisions
		WHERE DocumentId IN (SELECT ID FROM deleted)

	DELETE FROM Drafts
		WHERE DocumentId IN (SELECT ID FROM deleted)

	DELETE FROM RevisionTags
		WHERE DocumentId IN (SELECT ID FROM deleted)

	-- Now delete rows themselves
	DELETE FROM Documents
		WHERE Id IN (SELECT ID FROM deleted)
END
GO
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}