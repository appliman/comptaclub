if not exists (select * from sysobjects where name = 'RolesByUsers' and xtype = 'U')
Begin

CREATE TABLE [RolesByUsers] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_RolesByUsers] PRIMARY KEY ([Id])
);
End
GO

