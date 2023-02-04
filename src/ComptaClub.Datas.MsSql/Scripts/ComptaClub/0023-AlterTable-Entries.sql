if Col_Length('Entries','MemberId') is not null
Begin
	alter table [Entries] drop column [MemberId]
End
Go