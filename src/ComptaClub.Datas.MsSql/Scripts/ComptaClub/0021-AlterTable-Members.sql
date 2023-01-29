
if Col_Length('Members','Name') is null
Begin
	alter table [Members] add [Name] varchar(100) not null
End
Go

if Col_Length('Members','Email') is null
Begin
	alter table [Members] add [Email] varchar(100) not null
End
Go

if Col_Length('Members','LicenseNumber') is null
Begin
	alter table [Members] add [LicenseNumber] varchar(100) null
End
Go

if Col_Length('Members','LicenseTypeName') is null
Begin
	alter table [Members] add [LicenseTypeName] varchar(255) null
End
Go

if Col_Length('Members','CreationDate') is null
Begin
	alter table [Members] add [CreationDate] int not null
End
Go