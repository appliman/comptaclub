if Col_Length('Entries','Balance') is null
Begin
	alter table [Entries] add Balance bigint null
End
Go

update Entries
set Balance = 0
where Balance is null
Go

alter table [Entries] alter column Balance bigint not null
Go

