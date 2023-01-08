if Col_Length('Exercices','ClosedDate') is null
Begin
	alter table [Exercices] add ClosedDate int null
End
Go
