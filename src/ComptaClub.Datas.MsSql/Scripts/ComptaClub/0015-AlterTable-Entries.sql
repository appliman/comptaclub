if Col_Length('Entries','BalanceAmount') is not null
Begin
	alter table [Entries] drop column [BalanceAmount]
End
Go