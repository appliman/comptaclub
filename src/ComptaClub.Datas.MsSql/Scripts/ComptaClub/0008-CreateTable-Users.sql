if not exists (select * from sysobjects where name = 'Users' and xtype = 'U')
Begin

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Name] varchar(100) NOT NULL,
    [Email] varchar(200) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
End
GO


