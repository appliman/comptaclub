if not exists (select * from sysobjects where name = 'DocumentsByEntities' and xtype = 'U')
Begin

CREATE TABLE [DocumentsByEntities] (
    [Id] uniqueidentifier NOT NULL,
    [MetaEntity] uniqueidentifier NOT NULL,
    [EntitiyId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_DocumentByEntities] PRIMARY KEY ([Id])
);
End
GO

