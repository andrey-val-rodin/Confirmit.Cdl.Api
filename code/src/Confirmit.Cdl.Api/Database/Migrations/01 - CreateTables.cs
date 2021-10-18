using FluentMigrator;
using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Migrations
{
	[Migration(1)]
    [UsedImplicitly]
    // ReSharper disable StringLiteralTypo
    public class CreateTables : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"
/****** Object:  Table [dbo].[Companies]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Companies](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](64) NULL,
 CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

/****** Object:  Table [dbo].[Drafts]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Drafts]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Drafts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentId] [bigint] NOT NULL,
	[DocumentName] [nvarchar](256) NOT NULL,
	[SourceCode] [nvarchar](max) NOT NULL,
	[SourceCodeEditOps] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NOT NULL,
	[Timestamp] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[EnduserLists]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnduserLists]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnduserLists](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_EnduserLists] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[EnduserPermissions]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnduserPermissions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnduserPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[EnduserId] [int] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_EnduserPermissions] PRIMARY KEY CLUSTERED 
(
	[ObjectId] ASC,
	[ObjectType] ASC,
	[EnduserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Endusers]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Endusers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Endusers](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[ListId] [int] NOT NULL,
 CONSTRAINT [PK_Endusers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Documents]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Documents](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedByName] [nvarchar](256) NOT NULL,
	[Created] [datetime] NOT NULL,
	[ModifiedBy] [int] NOT NULL,
	[ModifiedByName] [nvarchar](256) NOT NULL,
	[Modified] [datetime] NOT NULL,
	[CompanyId] [int] NULL,
 CONSTRAINT [PK_dbo.Documents] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Revisions]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Revisions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Revisions](
	[RevisionNum] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentId] [bigint] NOT NULL,
	[DocumentName] [nvarchar](256) NOT NULL,
	[SourceCode] [nvarchar](max) NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[CommitDetails] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.Revisions] PRIMARY KEY NONCLUSTERED 
(
	[RevisionNum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tags]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[IsDefault] [bit] NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[UserPermissions]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserPermissions](
	[ObjectId] [bigint] NOT NULL,
	[ObjectType] [tinyint] NOT NULL,
	[UserId] [int] NOT NULL,
	[Permission] [tinyint] NOT NULL,
 CONSTRAINT [PK_UserPermissions] PRIMARY KEY CLUSTERED 
(
	[ObjectId] ASC,
	[ObjectType] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Users]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Users](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[FirstName] [nvarchar](64) NULL,
	[LastName] [nvarchar](64) NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[RevisionTags]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RevisionTags]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RevisionTags](
	[DocumentId] [bigint] NOT NULL,
	[TagId] [int] NOT NULL,
	[RevisionNum] [bigint] NOT NULL,
 CONSTRAINT [PK_RevisionTags] PRIMARY KEY CLUSTERED 
(
	[DocumentId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Index [IX_RevisionDocumentId]    Script Date: 9/5/2017 1:57:43 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Revisions]') AND name = N'IX_RevisionDocumentId')
CREATE NONCLUSTERED INDEX [IX_RevisionDocumentId] ON [dbo].[Revisions]
(
	[DocumentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__EnduserPe__temp___0B91BA14]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[EnduserPermissions] ADD  DEFAULT ((0)) FOR [Permission]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__UserPermi__temp___0A9D95DB]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UserPermissions] ADD  DEFAULT ((0)) FOR [Permission]
END

GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Endusers_EnduserLists]') AND parent_object_id = OBJECT_ID(N'[dbo].[Endusers]'))
ALTER TABLE [dbo].[Endusers]  WITH CHECK ADD  CONSTRAINT [FK_Endusers_EnduserLists] FOREIGN KEY([ListId])
REFERENCES [dbo].[EnduserLists] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Endusers_EnduserLists]') AND parent_object_id = OBJECT_ID(N'[dbo].[Endusers]'))
ALTER TABLE [dbo].[Endusers] CHECK CONSTRAINT [FK_Endusers_EnduserLists]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Revision_Document]') AND parent_object_id = OBJECT_ID(N'[dbo].[Revisions]'))
ALTER TABLE [dbo].[Revisions]  WITH CHECK ADD  CONSTRAINT [FK_Revision_Document] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Documents] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Revision_Document]') AND parent_object_id = OBJECT_ID(N'[dbo].[Revisions]'))
ALTER TABLE [dbo].[Revisions] CHECK CONSTRAINT [FK_Revision_Document]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Users_Companies]') AND parent_object_id = OBJECT_ID(N'[dbo].[Users]'))
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Companies] FOREIGN KEY([CompanyId])
REFERENCES [dbo].[Companies] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Users_Companies]') AND parent_object_id = OBJECT_ID(N'[dbo].[Users]'))
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Companies]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RevisionTags_Document]') AND parent_object_id = OBJECT_ID(N'[dbo].[RevisionTags]'))
ALTER TABLE [dbo].[RevisionTags]  WITH CHECK ADD  CONSTRAINT [FK_RevisionTags_Document] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Documents] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RevisionTags_Document]') AND parent_object_id = OBJECT_ID(N'[dbo].[RevisionTags]'))
ALTER TABLE [dbo].[RevisionTags] CHECK CONSTRAINT [FK_RevisionTags_Document]
GO
/****** Object:  Trigger [dbo].[TR_Document_Delete]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_Document_Delete]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [dbo].[TR_Document_Delete] on [dbo].[Documents]
FOR DELETE
AS
BEGIN
	SET NOCOUNT ON

	DELETE FROM UserPermissions
    WHERE ObjectId IN (SELECT ID FROM deleted) AND ObjectType = 1

	DELETE FROM EnduserPermissions
    WHERE ObjectId IN (SELECT ID FROM deleted) AND ObjectType = 1
END
' 
GO
/****** Object:  Trigger [dbo].[TR_Document_Insert]    Script Date: 9/5/2017 1:57:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_Document_Insert]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [dbo].[TR_Document_Insert] 
   ON  [dbo].[Documents]
   FOR INSERT 
AS 
BEGIN
	SET NOCOUNT ON;

	INSERT INTO UserPermissions(ObjectId, ObjectType, UserId, Permission)
	SELECT inserted.Id, 1, inserted.CreatedBy, 3 -- Permission.Delete
	FROM inserted
END' 
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Tags] WHERE [name] = 'Published') 
BEGIN
INSERT INTO [dbo].[Tags]
           ([Name],[IsDefault])
     VALUES  ('Published', 1) 
END
");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}