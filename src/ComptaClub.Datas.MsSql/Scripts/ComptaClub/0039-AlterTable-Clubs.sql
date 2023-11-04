
alter table [Clubs] alter column [Name] nvarchar(100) not null
Go

if Col_Length('Clubs','LogoBase64String') is null
Begin
	alter table [Clubs] add [LogoBase64String] varchar(max) null
End
Go

if Col_Length('Clubs','LogoContentType') is null
Begin
	alter table [Clubs] add [LogoContentType] varchar(50) null
End
Go
