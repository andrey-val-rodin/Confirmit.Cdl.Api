using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(21, TransactionBehavior.None)]
    [UsedImplicitly]
    public class AllowSnapshotIsolationLevel : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
ALTER DATABASE CmlStorage  
SET ALLOW_SNAPSHOT_ISOLATION ON
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
