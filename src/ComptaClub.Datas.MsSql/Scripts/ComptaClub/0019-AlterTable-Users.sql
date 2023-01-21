if Col_Length('Users','CreationDate') is null
Begin
	alter table [Users] add [CreationDate] datetime2 not null
End
Go

if Col_Length('Users','DisableDate') is null
Begin
	alter table [Users] add [CreationDate] datetime2 null
End
Go