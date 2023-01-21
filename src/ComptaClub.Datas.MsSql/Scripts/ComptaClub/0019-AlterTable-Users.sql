if Col_Length('Users','CreationDate') is null
Begin
	alter table [Users] add [CreationDate] int not null
End
Go

