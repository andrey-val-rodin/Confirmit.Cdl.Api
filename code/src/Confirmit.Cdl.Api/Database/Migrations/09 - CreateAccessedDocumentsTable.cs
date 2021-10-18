using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(9)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class CreateAccessedDocumentsTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
CREATE TABLE [dbo].[AccessedDocuments](
    [Id] [bigint] NOT NULL,
    [UserId] [int] NOT NULL,
    [IsUser] [bit] NOT NULL,
    [Accessed] [datetime] NOT NULL,
 CONSTRAINT [PK_AccessedDocuments] PRIMARY KEY CLUSTERED
(
    [Id] ASC,
    [UserId] ASC,
    [IsUser] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
 
ALTER TABLE [dbo].[AccessedDocuments]  WITH CHECK ADD  CONSTRAINT [FK_AccessedDocuments_Documents] FOREIGN KEY([Id])
REFERENCES [dbo].[Documents] ([Id])
GO
 
ALTER TABLE [dbo].[AccessedDocuments] CHECK CONSTRAINT [FK_AccessedDocuments_Documents]
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
 
    DELETE FROM Revisions
        WHERE DocumentId IN (SELECT Id FROM deleted)
 
    DELETE FROM AccessedDocuments
        WHERE Id IN (SELECT Id FROM deleted)
 
    -- Now delete rows themselves
    DELETE FROM Documents
        WHERE Id IN (SELECT Id FROM deleted)
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