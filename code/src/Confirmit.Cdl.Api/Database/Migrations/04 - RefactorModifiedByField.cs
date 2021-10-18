using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(4)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class RefactorModifiedByField : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
/* ADD ModifiedBy to DRAFTS table, MIGRATE from DOCUMENTS  */
IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Drafts' AND column_name='ModifiedBy')
BEGIN
  alter table [Drafts] add [ModifiedBy] [int] NULL
END
GO

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Documents' AND column_name='Modified')
BEGIN
	UPDATE [Drafts] 
	SET [ModifiedBy] = (SELECT [ModifiedBy] FROM [Documents] docs WHERE [DocumentId] = docs.Id)	
END
GO
  
IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Drafts' AND column_name='ModifiedBy')
BEGIN
  alter table [Drafts]
	alter column [ModifiedBy] [int] NOT NULL
END
GO



/* ADD ModifiedByName to DRAFTS table, MIGRATE from DOCUMENTS  */
IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Drafts' AND column_name='ModifiedByName')
BEGIN
  alter table [Drafts] add [ModifiedByName] [nvarchar](256) NULL
END
GO

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Documents' AND column_name='ModifiedByName')
BEGIN
	UPDATE [Drafts] 
	SET [ModifiedByName] = (SELECT [ModifiedByName] FROM [Documents] docs WHERE [DocumentId] = docs.Id)	
END
GO
  
IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Drafts' AND column_name='ModifiedByName')
BEGIN
  alter table [Drafts]
	alter column [ModifiedByName] [nvarchar](256) NOT NULL
END
GO



/* ADD ModifiedBy to Revisions table, MIGRATE from DOCUMENTS  */
IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Revisions' AND column_name='ModifiedBy')
BEGIN
  alter table [Revisions] add [ModifiedBy] [int] NULL
END
GO

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Documents' AND column_name='Modified')
BEGIN
	UPDATE [Revisions] 
	SET [ModifiedBy] = (SELECT [ModifiedBy] FROM [Documents] docs WHERE [DocumentId] = docs.Id)	
END
GO
  
IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Revisions' AND column_name='ModifiedBy')
BEGIN
  alter table [Revisions]
	alter column [ModifiedBy] [int] NOT NULL
END
GO



/* ADD ModifiedByName to Revisions table, MIGRATE from DOCUMENTS  */
IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Revisions' AND column_name='ModifiedByName')
BEGIN
  alter table [Revisions] add [ModifiedByName] [nvarchar](256) NULL
END
GO

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Documents' AND column_name='ModifiedByName')
BEGIN
	UPDATE [Revisions] 
	SET [ModifiedByName] = (SELECT [ModifiedByName] FROM [Documents] docs WHERE [DocumentId] = docs.Id)	
END
GO
  
IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Revisions' AND column_name='ModifiedByName')
BEGIN
  alter table [Revisions]
	alter column [ModifiedByName] [nvarchar](256) NOT NULL
END
GO


/* REMOVE Modified from DOCUMENTS table  */
IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Documents' AND column_name='Modified')
BEGIN
  alter table Documents drop column [Modified]
END


/* REMOVE ModifiedBy from DOCUMENTS table  */

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Documents' AND column_name='ModifiedBy')
BEGIN
  alter table Documents drop column [ModifiedBy]
END

/* REMOVE ModifiedByName from DOCUMENTS table  */

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Documents' AND column_name='ModifiedByName')
BEGIN
  alter table Documents drop column [ModifiedByName]
END

");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}