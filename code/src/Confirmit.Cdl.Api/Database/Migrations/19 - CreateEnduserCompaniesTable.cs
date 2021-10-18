using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
    [Migration(19)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class CreateEnduserCompaniesTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
-- Add new columns to Endusers table
ALTER TABLE [dbo].[Endusers] ADD
    [CompanyId] [int] NOT NULL DEFAULT 0,
	[Email] [nvarchar](254) NULL
GO

-- Create EnduserCompanies table
CREATE TABLE [dbo].[EnduserCompanies](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_EnduserCompanies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Insert dummy company
INSERT INTO [dbo].[EnduserCompanies] ([Id], [Name])
VALUES (0, null)

-- Add foreign constraint
ALTER TABLE [dbo].[Endusers]  WITH CHECK ADD  CONSTRAINT [FK_Endusers_EnduserCompanies] FOREIGN KEY([CompanyId])
REFERENCES [dbo].[EnduserCompanies] ([Id])
GO

ALTER TABLE [dbo].[Endusers] CHECK CONSTRAINT [FK_Endusers_EnduserCompanies]
GO
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}