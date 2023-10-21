if Col_Length('DocumentsByEntities','CreationDate') is null
Begin
	alter table [DocumentsByEntities] add CreationDate datetime2 null
End
Go

update DocumentsByEntities
set CreationDate = GetDate()
where DocumentId is null
Go

alter table [DocumentsByEntities] alter column CreationDate datetime2 not null
Go

