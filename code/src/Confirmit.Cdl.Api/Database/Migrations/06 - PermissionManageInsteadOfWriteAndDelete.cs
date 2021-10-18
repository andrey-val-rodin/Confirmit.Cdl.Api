using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(6)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class PermissionManageInsteadOfWriteAndDelete : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- Update UserPermissions table
UPDATE [UserPermissions] SET [Permission] = 2 WHERE [Permission] > 2
GO

-- Change trigger
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
		2					-- Permission.Manage
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