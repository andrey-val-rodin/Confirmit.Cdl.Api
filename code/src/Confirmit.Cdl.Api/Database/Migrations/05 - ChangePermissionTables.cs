using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(5)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class ChangePermissionTables : Migration
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

-- Create table EnduserPermissions with new structure
CREATE TABLE [EnduserPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[AccountId] [int] NOT NULL,
	[AccountType] [tinyint] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_EnduserPermissions] PRIMARY KEY CLUSTERED 
(
	[ObjectId] ASC,
	[ObjectType] ASC,
	[AccountId] ASC,
	[AccountType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- So far, AppStudio has set permissions for all endusers in enduser list,
-- therefore we can replace individual permissions by permission of the whole enduser list
INSERT INTO [EnduserPermissions]
SELECT DISTINCT ObjectId, ObjectType, ListId AS AccountId, 2 AS AccountType, Permission
  FROM [OldEnduserPermissions]
  INNER JOIN [Endusers] ON EnduserId=Id
GO

-- Delete old table
DROP TABLE [OldEnduserPermissions]
GO

-- UserPermissions table
-- Rename old table and key
EXECUTE sp_rename N'UserPermissions', N'OldUserPermissions', 'OBJECT'
EXECUTE sp_rename N'PK_UserPermissions', N'PK_OldUserPermissions', 'OBJECT'
GO

-- Create table UserPermissions with new structure
CREATE TABLE [UserPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[AccountId] [int] NOT NULL,
	[AccountType] [tinyint] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_UserPermissions] PRIMARY KEY CLUSTERED 
(
	[ObjectId] ASC,
	[ObjectType] ASC,
	[AccountId] ASC,
	[AccountType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Copy table
INSERT INTO [UserPermissions]
SELECT ObjectId, ObjectType, UserId AS AccountId, 1 AS AccountType, Permission
  FROM [OldUserPermissions]
GO

-- Delete old table
DROP TABLE [OldUserPermissions]
GO

-- Finally, change trigger
ALTER TRIGGER [dbo].[TR_Document_Insert] 
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
		3					-- Permission.Delete
	FROM inserted
END
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}