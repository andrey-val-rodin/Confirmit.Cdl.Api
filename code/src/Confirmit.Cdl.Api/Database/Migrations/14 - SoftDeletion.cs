using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(14)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class SoftDeletion : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
ALTER TABLE dbo.Documents ADD
	Deleted datetime NULL,
	DeletedBy int NULL,
	DeletedByName nvarchar(256) NULL
GO
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}