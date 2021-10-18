using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(10)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class Commits : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
CREATE TABLE [dbo].[Commits](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [DocumentId] [bigint] NOT NULL,
    [RevisionId] [bigint] NULL,
	[RevisionNumber] [int] NULL,
    [Action] [tinyint] NOT NULL,
    [Created] [datetime] NOT NULL,
    [CreatedBy] [int] NOT NULL,
    [CreatedByName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_Commits] PRIMARY KEY CLUSTERED
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
 
ALTER TABLE [dbo].[Commits]  WITH CHECK ADD  CONSTRAINT [FK_Commits_Documents] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Documents] ([Id])
GO
 
ALTER TABLE [dbo].[Commits] CHECK CONSTRAINT [FK_Commits_Documents]
GO
 
ALTER TABLE [dbo].[Commits]  WITH CHECK ADD  CONSTRAINT [FK_Commits_Revisions] FOREIGN KEY([RevisionId])
REFERENCES [dbo].[Revisions] ([Id])
GO
 
ALTER TABLE [dbo].[Commits] CHECK CONSTRAINT [FK_Commits_Revisions]
GO


ALTER TRIGGER [dbo].[TR_Document_Delete] ON [dbo].[Documents]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON
 
    DELETE FROM UserPermissions
        WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1
 
    DELETE FROM EnduserPermissions
        WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1
 
    DELETE FROM Commits
        WHERE DocumentId IN (SELECT Id FROM deleted)
 
    DELETE FROM Revisions
        WHERE DocumentId IN (SELECT Id FROM deleted)
 
    DELETE FROM AccessedDocuments
        WHERE Id IN (SELECT Id FROM deleted)
 
    -- Now delete rows themselves
    DELETE FROM Documents
        WHERE Id IN (SELECT Id FROM deleted)
END");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}