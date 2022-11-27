if not exists (select * from sysobjects where name = 'Roles' and xtype = 'U')
Begin

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] varchar(100) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
End
GO


