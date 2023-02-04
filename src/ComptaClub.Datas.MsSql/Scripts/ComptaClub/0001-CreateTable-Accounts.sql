if not exists (select * from sysobjects where name = 'Accounts' and xtype = 'U')
Begin

    CREATE TABLE [Accounts] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Direction] int NOT NULL,
        [Label] nvarchar(1024) NOT NULL,
        [CreationDate] int NOT NULL,
        [ParentAccountId] uniqueidentifier NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
    );
End
GO