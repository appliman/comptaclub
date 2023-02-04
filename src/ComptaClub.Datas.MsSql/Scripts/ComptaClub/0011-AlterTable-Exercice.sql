if Col_Length('Exercices','BalanceAmount') is null
Begin
	alter table [Exercices] add BalanceAmount bigint  null
End
Go

update Exercices
set BalanceAmount = 0
where BalanceAmount is null
Go

alter table [Exercices] alter column BalanceAmount bigint not null
Go

