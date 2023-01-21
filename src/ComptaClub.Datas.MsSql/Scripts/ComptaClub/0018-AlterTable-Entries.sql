if Col_Length('Entries','UserCreatorId') is not null
Begin
	alter table [Entries] alter column [UserCreatorId] uniqueidentifier not null
End
Go