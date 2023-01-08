if Col_Length('Entries','ImportId') is null
Begin
	alter table [Entries] add ImportId varchar(1024) null
End
Go