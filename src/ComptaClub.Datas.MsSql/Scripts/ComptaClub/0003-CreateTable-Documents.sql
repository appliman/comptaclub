if not exists (select * from sysobjects where name = 'Documents' and xtype = 'U')
Begin
CREATE TABLE [Documents] (
    [Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
);
End
GO
