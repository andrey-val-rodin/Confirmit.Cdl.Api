using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(18)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class CreateSelectedEnduserListsTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- Create new table
CREATE TABLE [dbo].[SelectedEnduserLists](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[ListId] [int] NOT NULL
 CONSTRAINT [PK_SelectedEnduserLists] PRIMARY KEY CLUSTERED 
(
	[ListId] ASC,
	[ObjectId] ASC,
	[ObjectType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SelectedEnduserLists]  WITH CHECK ADD  CONSTRAINT [FK_SelectedEnduserLists_EnduserLists] FOREIGN KEY([ListId])
REFERENCES [dbo].[EnduserLists] ([Id])
GO

ALTER TABLE [dbo].[SelectedEnduserLists] CHECK CONSTRAINT [FK_SelectedEnduserLists_EnduserLists]
GO

-- Copy existent enduser lists
INSERT INTO [dbo].[SelectedEnduserLists]
	SELECT 
		ObjectId,
		ObjectType,
		ListId
		FROM [dbo].[EnduserPermissions]
		INNER JOIN Endusers ON Endusers.Id = UserId
			WHERE (1 = ObjectType)
	UNION
	SELECT
		ObjectId,
		ObjectType,
		ListId
		FROM [dbo].[EnduserListPermissions]
GO

-- Change trigger on delete
ALTER TRIGGER [dbo].[TR_Document_Delete]
	ON [dbo].[Documents]
	FOR DELETE
AS
BEGIN
	SET NOCOUNT ON

	DELETE FROM UserPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

	DELETE FROM CompanyPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

	DELETE FROM EnduserPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

	DELETE FROM EnduserListPermissions
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1

	DELETE FROM SelectedEnduserLists
		WHERE ObjectId IN (SELECT Id FROM deleted) AND ObjectType = 1
END

");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}