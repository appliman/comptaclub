if not exists (select * from sysobjects where name = 'Exercices' and xtype = 'U')
Begin

CREATE TABLE [Exercices] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [Label] nvarchar(max) NULL,
    [InitialAmount] bigint NOT NULL,
    [StartDate] int NOT NULL,
    [EndDate] int NOT NULL,
    [CreationDate] int NOT NULL,
    [LastEntryId] uniqueidentifier NULL,
    [Active] bit NOT NULL,
    CONSTRAINT [PK_Exercices] PRIMARY KEY ([Id])
);
End
GO
