if not exists (select * from sysobjects where name = 'Members' and xtype = 'U')
Begin

CREATE TABLE [Members] (
    [Id] uniqueidentifier NOT NULL,
    Name varchar(100) not null,
    Email varchar(200) not null,
    LicenseNumber varchar(100) null,
    LicenseTypeName varchar(255) null,
    CreationDate int not null,
    CONSTRAINT [PK_Members] PRIMARY KEY ([Id])

);
End
GO
