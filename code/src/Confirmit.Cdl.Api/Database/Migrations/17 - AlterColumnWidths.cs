using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(17)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class AlterColumnWidths : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
ALTER TABLE Users
	ALTER COLUMN [Name] [nvarchar](254) NOT NULL
GO

ALTER TABLE Endusers
	ALTER COLUMN [Name] [nvarchar](254) NOT NULL
GO
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}