using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(16)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class AddOriginDocumentId : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
ALTER TABLE Documents
    ADD [OriginDocumentId] [bigint] NULL");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}