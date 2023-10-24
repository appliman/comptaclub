if not exists (select * from sysobjects where name = 'Clubs' and xtype = 'U')
Begin

CREATE TABLE [Clubs] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(10) NOT NULL,
    [Object] nvarchar(1024) NULL,
    [CreationDate] int NOT NULL,
    [Address] nvarchar(1024) null,
    [SiretNumber] nvarchar(20) NULL,
    [SirenNumber] nvarchar(20) NULL,
    [Rna] nvarchar(20) NULL,
    [Email] nvarchar(255) NULL,
    [WebSite] nvarchar(512) NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [ContactName] nvarchar(255) NULL
    CONSTRAINT [PK_Clubs] PRIMARY KEY ([Id])
);
End
GO
