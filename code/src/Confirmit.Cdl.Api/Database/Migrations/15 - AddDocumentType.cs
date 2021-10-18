using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(15)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class AddDocumentType : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
ALTER TABLE Documents
    ADD [Type] [tinyint] NOT NULL DEFAULT 1");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}