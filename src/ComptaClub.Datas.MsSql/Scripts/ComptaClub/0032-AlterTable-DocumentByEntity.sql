if Col_Length('DocumentsByEntities','DocuemntId') is null
Begin
	alter table [DocumentsByEntities] add DocumentId uniqueidentifier null
End
Go

update DocumentsByEntities
set DocumentId = Newid()
where DocumentId is null
Go

alter table [DocumentsByEntities] alter column DocumentId uniqueidentifier not null
Go

