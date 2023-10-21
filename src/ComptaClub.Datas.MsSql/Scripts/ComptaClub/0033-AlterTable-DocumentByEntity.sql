if Col_Length('DocumentsByEntities','EntitiyId') is not null
Begin
	exec sp_rename 'DocumentsByEntities.EntitiyId', 'EntityId', 'COLUMN'
End
Go