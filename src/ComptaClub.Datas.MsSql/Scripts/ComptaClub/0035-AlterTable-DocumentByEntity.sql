if Col_Length('DocumentsByEntities','MetaEntity') is not null
Begin
	alter table [DocumentsByEntities] drop column MetaEntity
End
Go

if Col_Length('DocumentsByEntities','MetaEntity') is null
Begin
	alter table [DocumentsByEntities] add MetaEntity int null
End
Go

update DocumentsByEntities
set MetaEntity = 0
where MetaEntity is null	
Go

alter table [DocumentsByEntities] alter column MetaEntity int not null
Go
