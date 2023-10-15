if not exists (select * from sysobjects where name = 'IncomeStatements' and xtype = 'U')
Begin

CREATE TABLE [IncomeStatements] (
    [Id] uniqueidentifier NOT NULL,
    [ExerciceId] uniqueidentifier NOT NULL,
    [CreditTotal] bigint not null,
    [DebitTotal] bigint not null,
    [Description] nvarchar(1024) NULL,
    [CreationDate] int NOT NULL,
    CONSTRAINT [PK_IncomeStatements] PRIMARY KEY ([Id])
);
End
GO

if not exists (select * from sysobjects where name = 'IncomeStatementItems' and xtype = 'U')
Begin

CREATE TABLE [IncomeStatementItems] (
    [Id] uniqueidentifier NOT NULL,
    [IncomeStatementId] uniqueidentifier NOT NULL,
    [AccountId] uniqueidentifier NOT NULL,
    [Amount] bigint not null,
    [Code] nvarchar(50) NULL,
    [Label] nvarchar(1024) NULL,
    [Direction] int NOT NULL,
    CONSTRAINT [PK_IncomeStatementItems] PRIMARY KEY ([Id])
);
End
GO