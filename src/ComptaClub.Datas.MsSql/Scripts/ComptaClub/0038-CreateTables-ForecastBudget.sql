if not exists (select * from sysobjects where name = 'ForecastBudgets' and xtype = 'U')
Begin

CREATE TABLE [ForecastBudgets] (
    [Id] uniqueidentifier not NULL,
    [IncomeStatementId] uniqueidentifier NULL,
    [Name] nvarchar(200) not NULL,
    [Description] nvarchar(max) NULL,
    [CreationDate] int not null,
    [CreditTotal] bigint not null,
    [DebitTotal] bigint not null,
    [IncomeStatementCreditTotal] bigint not null,
    [IncomeStatementDebitTotal] bigint not null,
    CONSTRAINT [PK_ForecastBudgets] PRIMARY KEY ([Id])
);
End
GO

if not exists (select * from sysobjects where name = 'ForecastBudgetItems' and xtype = 'U')
Begin

CREATE TABLE [ForecastBudgetItems] (
    [Id] uniqueidentifier not NULL,
    [ForecastBudgetId] uniqueidentifier not NULL,
    [ParentForecastBudgetItemId] uniqueidentifier NULL,
    [AccountId] uniqueidentifier not NULL,
    [AccountCode] nvarchar(50) not NULL,
    [AccountLabel] nvarchar(1024) not NULL,
    [IncomeStatementAmount] bigint null,
    [Amount] bigint not null,
    [Direction] int NOT NULL,
    CONSTRAINT [PK_ForecastBudgetItems] PRIMARY KEY ([Id])
);
End
GO
