using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(13)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class SplitPermissionTables : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- EnduserPermissions table
-- Rename old table and key
EXECUTE sp_rename N'EnduserPermissions', N'OldEnduserPermissions', 'OBJECT'
EXECUTE sp_rename N'PK_EnduserPermissions', N'PK_OldEnduserPermissions', 'OBJECT'
GO

-- Create new table EnduserPermissions with different order in constraint
CREATE TABLE [dbo].[EnduserPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[UserId] [int] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_EnduserPermissions] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ObjectId] ASC,
	[ObjectType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[EnduserPermissions]  WITH CHECK ADD  CONSTRAINT [FK_EnduserPermissions_Endusers] FOREIGN KEY([UserId])
REFERENCES [dbo].[Endusers] ([Id])
GO

ALTER TABLE [dbo].[EnduserPermissions] CHECK CONSTRAINT [FK_EnduserPermissions_Endusers]
GO

-- Copy table
INSERT INTO [EnduserPermissions]
SELECT ObjectId, ObjectType, AccountId AS UserId, Permission
  FROM [OldEnduserPermissions]
  WHERE AccountType = 1 AND AccountId IN (SELECT Id FROM Endusers)
GO

-- Create new table EnduserListPermissions
CREATE TABLE [dbo].[EnduserListPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[ListId] [int] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_EnduserListPermissions] PRIMARY KEY CLUSTERED 
(
	[ListId] ASC,
	[ObjectId] ASC,
	[ObjectType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[EnduserListPermissions]  WITH CHECK ADD  CONSTRAINT [FK_EnduserListPermissions_EnduserLists] FOREIGN KEY([ListId])
REFERENCES [dbo].[EnduserLists] ([Id])
GO

ALTER TABLE [dbo].[EnduserListPermissions] CHECK CONSTRAINT [FK_EnduserListPermissions_EnduserLists]
GO

-- Copy table
INSERT INTO [EnduserListPermissions]
SELECT ObjectId, ObjectType, AccountId AS ListId, Permission
  FROM [OldEnduserPermissions]
  WHERE AccountType = 2 AND AccountId IN (SELECT Id FROM EnduserLists)
GO

-- Delete old table
DROP TABLE [OldEnduserPermissions]
GO

-- UserPermissions table
-- Rename old table and key
EXECUTE sp_rename N'UserPermissions', N'OldUserPermissions', 'OBJECT'
EXECUTE sp_rename N'PK_UserPermissions', N'PK_OldUserPermissions', 'OBJECT'
GO

-- Create new table UserPermissions with different order in constraint
CREATE TABLE [dbo].[UserPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[UserId] [int] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_UserPermissions] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ObjectId] ASC,
	[ObjectType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UserPermissions]  WITH CHECK ADD  CONSTRAINT [FK_UserPermissions_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[UserPermissions] CHECK CONSTRAINT [FK_UserPermissions_Users]
GO

-- Copy table
INSERT INTO [UserPermissions]
SELECT ObjectId, ObjectType, AccountId AS UserId, Permission
  FROM [OldUserPermissions]
  WHERE AccountType = 1 AND AccountId IN (SELECT Id FROM Users)
GO

-- Create new table CompanyPermissions
CREATE TABLE [dbo].[CompanyPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_CompanyPermissions] PRIMARY KEY CLUSTERED 
(
	[CompanyId] ASC,
	[ObjectId] ASC,
	[ObjectType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompanyPermissions]  WITH CHECK ADD  CONSTRAINT [FK_CompanyPermissions_Companies] FOREIGN KEY([CompanyId])
REFERENCES [dbo].[Companies] ([Id])
GO

ALTER TABLE [dbo].[CompanyPermissions] CHECK CONSTRAINT [FK_CompanyPermissions_Companies]
GO

-- Copy table
INSERT INTO [CompanyPermissions]
SELECT ObjectId, ObjectType, AccountId AS CompanyId, Permission
  FROM [OldUserPermissions]
  WHERE AccountType = 2 AND AccountId IN (SELECT Id FROM Companies)
GO

-- Delete old table
DROP TABLE [OldUserPermissions]
GO

ALTER TRIGGER [dbo].[TR_Document_Insert] 
   ON  [dbo].[Documents]
   FOR INSERT 
AS 
BEGIN
	SET NOCOUNT ON;

	INSERT INTO UserPermissions(ObjectId, ObjectType, UserId, Permission)
	SELECT
		inserted.Id,		-- Document Id
		1,					-- Document type
		inserted.CreatedBy, -- User Id
		2					-- Permission.Manage
	FROM inserted
END
GO

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