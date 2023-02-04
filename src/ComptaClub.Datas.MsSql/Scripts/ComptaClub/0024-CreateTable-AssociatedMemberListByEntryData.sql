if not exists (select * from sysobjects where name = 'AssociatedMemberListByEntries' and xtype = 'U')
Begin

CREATE TABLE [AssociatedMemberListByEntries] (
    [Id] uniqueidentifier NOT NULL,
    [MemberId] uniqueidentifier NOT NULL,
    [EntryId] uniqueidentifier NOT NULL,
    [CreationDate] int not null,
    CONSTRAINT [PK_AssociatedMemberListByEntries] PRIMARY KEY ([Id])
);
End
GO

