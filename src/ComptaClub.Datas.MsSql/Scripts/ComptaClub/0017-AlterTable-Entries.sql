if Col_Length('Entries','DeletedDate') is null
Begin
	alter table [Entries] add DeletedDate int null
End
Go