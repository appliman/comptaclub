if not exists (select * from sysobjects where name = 'Documents' and xtype = 'U')
Begin
CREATE TABLE [Documents] (
    [Id] uniqueidentifier NOT NULL,
    CreationDate int not null,
    LastUpdate int not null,
    FileName varchar(1024) not null,
    Description varchar(2048) null,
    MimeType varchar(100) null,
    Size bigint not null,
    UserOwnerId uniqueidentifier null,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
);
End
GO
