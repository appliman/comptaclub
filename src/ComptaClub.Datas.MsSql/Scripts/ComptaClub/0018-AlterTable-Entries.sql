update Entries
set UserCreatorId = '00000000-0000-0000-0000-000000000000'
where UserCreatorId is null

if Col_Length('Entries','UserCreatorId') is not null
Begin
	alter table [Entries] alter column [UserCreatorId] uniqueidentifier not null
End
Go