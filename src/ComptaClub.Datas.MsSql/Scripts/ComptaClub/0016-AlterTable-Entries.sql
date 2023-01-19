if Col_Length('Entries','BalanceValue') is not null
Begin
	alter table [Entries] drop column [BalanceValue]
End
Go