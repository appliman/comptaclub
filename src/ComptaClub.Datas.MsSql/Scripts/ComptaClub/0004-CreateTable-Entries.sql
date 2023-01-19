if not exists (select * from sysobjects where name = 'Entries' and xtype = 'U')
Begin

CREATE TABLE [Entries] (
    [Id] uniqueidentifier NOT NULL,
    [PartNumber] nvarchar(max) NOT NULL,
    [Label] nvarchar(max) NOT NULL,
    [CreationDate] int NOT NULL,
    [ValueDate] int NOT NULL,
    [DeletedDate] int NULL,
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
End
GO
