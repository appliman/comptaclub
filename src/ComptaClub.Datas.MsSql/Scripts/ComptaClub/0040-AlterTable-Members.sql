
if Col_Length('Members','State') is null
Begin
	alter table [Members] add [State] int null
End
Go

update Members set State = 1 where State is null
Go

alter table [Members] alter column [State] int not null	
Go

