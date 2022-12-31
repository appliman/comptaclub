if Col_Length('Entries','ValueDate') is null
Begin
	alter table [Entries] add ValueDate int  null
End
Go

update Entries
set ValueDate = CreationDate
where ValueDate is null
Go

alter table [Entries] alter column ValueDate int not null
Go

