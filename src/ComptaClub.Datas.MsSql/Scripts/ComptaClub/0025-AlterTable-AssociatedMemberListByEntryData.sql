if Col_Length('AssociatedMemberListByEntries','Amount') is null
Begin
	alter table [AssociatedMemberListByEntries] add Amount bigint null
End
Go

update AssociatedMemberListByEntries
set amount = 0
where amount is null
Go

alter table [AssociatedMemberListByEntries] alter column Amount bigint not null
Go