if Col_Length('Users','DisableDate') is null
Begin
	alter table [Users] add [DisableDate] int null
End
Go