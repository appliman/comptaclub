if Col_Length('Banks','Active') is null
Begin
	alter table [Banks] add Active bit null
End
Go

update Banks
set Active = 0
where Active is null
Go 

alter table [Banks] alter column Active bit not null
Go
