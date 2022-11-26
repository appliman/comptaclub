CREATE TABLE [Accounts] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [Direction] int NOT NULL,
    [Label] nvarchar(max) NOT NULL,
    [CreationDate] int NOT NULL,
    [ParentAccountId] uniqueidentifier NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Banks] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [Label] nvarchar(max) NULL,
    [CreationDate] int NOT NULL,
    CONSTRAINT [PK_Banks] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Documents] (
    [Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Entries] (
    [Id] uniqueidentifier NOT NULL,
    [PartNumber] nvarchar(max) NOT NULL,
    [Label] nvarchar(max) NOT NULL,
    [CreationDate] int NOT NULL,
    [BalanceValue] bigint NOT NULL,
    [Amount] bigint NOT NULL,
    [AccountDirection] int NOT NULL,
    [BankId] uniqueidentifier NOT NULL,
    [AccountId] uniqueidentifier NOT NULL,
    [ExerciceId] uniqueidentifier NOT NULL,
    [UserCreatorId] uniqueidentifier NULL,
    [MemberId] uniqueidentifier NULL,
    [PaymentType] int NOT NULL,
    [ExtraInfos] nvarchar(max) NULL,
    CONSTRAINT [PK_Entries] PRIMARY KEY ([Id])
);
GO


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
GO


CREATE TABLE [Members] (
    [Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [RolesByUsers] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_RolesByUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


