if not exists (select * from sysobjects where name = 'DocumentContents' and xtype = 'U')
Begin
CREATE TABLE [DocumentContents] (
    [DocumentId] uniqueidentifier NOT NULL,
    Content image not null,
    CONSTRAINT [PK_DocumentContents] PRIMARY KEY ([DocumentId])
);
End
GO
