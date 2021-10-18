using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(12)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class Aliases : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
CREATE TABLE [dbo].[Aliases](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Namespace] [nvarchar](80) NOT NULL,
	[Alias] [nvarchar](50) NOT NULL,
	[DocumentId] [bigint] NOT NULL,
 CONSTRAINT [PK_Aliases] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Aliases]  WITH CHECK ADD  CONSTRAINT [FK_Aliases_Documents] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Documents] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Aliases] CHECK CONSTRAINT [FK_Aliases_Documents]
GO
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}