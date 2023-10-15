if Col_Length('IncomeStatementItems','ParentIncomeStatementItemId') is null
Begin
	alter table [IncomeStatementItems] add [ParentIncomeStatementItemId] uniqueidentifier null
End
Go