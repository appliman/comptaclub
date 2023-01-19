if not exists (select * from sysobjects where name = 'Banks' and xtype = 'U')
Begin

CREATE TABLE [Banks] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Label] nvarchar(1024) NULL,
    [CreationDate] int NOT NULL,
    [Active] bit not null,
    CONSTRAINT [PK_Banks] PRIMARY KEY ([Id])
);
End
GO
