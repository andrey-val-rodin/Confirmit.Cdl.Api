using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(20)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class FixDbConcurrencyViolation : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- Rename columns
EXEC sp_RENAME 'UserPermissions.ObjectId', 'DocumentId', 'COLUMN'
EXEC sp_RENAME 'CompanyPermissions.ObjectId', 'DocumentId', 'COLUMN'
EXEC sp_RENAME 'EnduserPermissions.ObjectId', 'DocumentId', 'COLUMN'
EXEC sp_RENAME 'EnduserListPermissions.ObjectId', 'DocumentId', 'COLUMN'
EXEC sp_RENAME 'SelectedEnduserLists.ObjectId', 'DocumentId', 'COLUMN'
GO

-- Change primary keys
ALTER TABLE UserPermissions DROP CONSTRAINT PK_UserPermissions
ALTER TABLE UserPermissions
   ADD CONSTRAINT PK_UserPermissions PRIMARY KEY CLUSTERED (DocumentId, UserId)
GO

ALTER TABLE CompanyPermissions DROP CONSTRAINT PK_CompanyPermissions
ALTER TABLE CompanyPermissions
   ADD CONSTRAINT PK_CompanyPermissions PRIMARY KEY CLUSTERED (DocumentId, CompanyId)
GO

ALTER TABLE EnduserPermissions DROP CONSTRAINT PK_EnduserPermissions
ALTER TABLE EnduserPermissions
   ADD CONSTRAINT PK_EnduserPermissions PRIMARY KEY CLUSTERED (DocumentId, UserId)
GO

ALTER TABLE EnduserListPermissions DROP CONSTRAINT PK_EnduserListPermissions
ALTER TABLE EnduserListPermissions
   ADD CONSTRAINT PK_EnduserListPermissions PRIMARY KEY CLUSTERED (DocumentId, ListId)
GO

ALTER TABLE SelectedEnduserLists DROP CONSTRAINT PK_SelectedEnduserLists
ALTER TABLE SelectedEnduserLists
   ADD CONSTRAINT PK_SelectedEnduserLists PRIMARY KEY CLUSTERED (DocumentId, ListId)
GO

-- Delete columns
ALTER TABLE UserPermissions DROP COLUMN ObjectType;
ALTER TABLE CompanyPermissions DROP COLUMN ObjectType;
ALTER TABLE EnduserPermissions DROP COLUMN ObjectType;
ALTER TABLE EnduserListPermissions DROP COLUMN ObjectType;
ALTER TABLE SelectedEnduserLists DROP COLUMN ObjectType;
GO

-- Create foreign constraints
ALTER TABLE UserPermissions  WITH CHECK ADD  CONSTRAINT FK_UserPermissions_Documents FOREIGN KEY(DocumentId)
REFERENCES Documents ([Id])
ON DELETE CASCADE
GO
ALTER TABLE UserPermissions CHECK CONSTRAINT FK_UserPermissions_Documents
GO

ALTER TABLE CompanyPermissions  WITH CHECK ADD  CONSTRAINT FK_CompanyPermissions_Documents FOREIGN KEY(DocumentId)
REFERENCES Documents ([Id])
ON DELETE CASCADE
GO
ALTER TABLE CompanyPermissions CHECK CONSTRAINT FK_CompanyPermissions_Documents
GO

ALTER TABLE EnduserPermissions  WITH CHECK ADD  CONSTRAINT FK_EnduserPermissions_Documents FOREIGN KEY(DocumentId)
REFERENCES Documents ([Id])
ON DELETE CASCADE
GO
ALTER TABLE EnduserPermissions CHECK CONSTRAINT FK_EnduserPermissions_Documents
GO

ALTER TABLE EnduserListPermissions  WITH CHECK ADD  CONSTRAINT FK_EnduserListPermissions_Documents FOREIGN KEY(DocumentId)
REFERENCES Documents ([Id])
ON DELETE CASCADE
GO
ALTER TABLE EnduserListPermissions CHECK CONSTRAINT FK_EnduserListPermissions_Documents
GO

ALTER TABLE SelectedEnduserLists  WITH CHECK ADD  CONSTRAINT FK_SelectedEnduserLists_Documents FOREIGN KEY(DocumentId)
REFERENCES Documents ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SelectedEnduserLists] CHECK CONSTRAINT FK_SelectedEnduserLists_Documents
GO

ALTER TRIGGER [dbo].[TR_Document_Insert] 
   ON  [dbo].[Documents]
   FOR INSERT 
AS 
BEGIN
	SET NOCOUNT ON;

	INSERT INTO UserPermissions(DocumentId, UserId, Permission)
	SELECT
		inserted.Id,		-- Document Id
		inserted.CreatedBy, -- User Id
		2					-- Permission.Manage
	FROM inserted
END
GO

DROP TRIGGER [dbo].[TR_Document_Delete]
GO
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}