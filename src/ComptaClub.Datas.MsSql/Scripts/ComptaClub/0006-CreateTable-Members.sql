if not exists (select * from sysobjects where name = 'Members' and xtype = 'U')
Begin

CREATE TABLE [Members] (
    [Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY ([Id])
);
End
GO
