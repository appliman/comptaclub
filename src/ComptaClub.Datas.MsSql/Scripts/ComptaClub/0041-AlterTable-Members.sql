
if Col_Length('Members','LastUpdate') is null
Begin
	alter table [Members] add [LastUpdate] int null
End
Go

update Members set LastUpdate = CreationDate where LastUpdate is null
Go

alter table [Members] alter column [LastUpdate] int not null	
Go

