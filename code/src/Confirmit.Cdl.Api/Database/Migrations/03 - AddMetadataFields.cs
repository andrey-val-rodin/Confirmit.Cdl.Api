using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(3)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class AddMetadataFields : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Drafts' AND column_name='PublicMetadata')
BEGIN
  alter table Drafts add PublicMetadata [nvarchar](512) null
END

IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Drafts' AND column_name='PrivateMetadata')
BEGIN
  alter table Drafts add PrivateMetadata [nvarchar](512) null
END

IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Revisions' AND column_name='PublicMetadata')
BEGIN
  alter table Revisions add PublicMetadata [nvarchar](512) null
END

IF NOT EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Revisions' AND column_name='PrivateMetadata')
BEGIN
  alter table Revisions add PrivateMetadata [nvarchar](512) null
END

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Drafts' AND column_name='Status')
BEGIN
  ALTER TABLE Drafts DROP COLUMN [Status]
END

IF EXISTS (SELECT NULL FROM information_schema.columns WHERE table_name='Revisions' AND column_name='CommitDetails')
BEGIN
  ALTER TABLE Revisions DROP COLUMN [CommitDetails]
END



");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}