if not exists (select * from sysobjects where name = 'DataProtectionKeys' and xtype = 'U')
Begin

CREATE TABLE [DataProtectionKeys] (
    [Id] int identity(1,1) not NULL,
    [FriendlyName] nvarchar(max) NULL,
    [Xml] nvarchar(max) NULL
    CONSTRAINT [PK_DataProtectionKeys] PRIMARY KEY ([Id])
);
End
GO
