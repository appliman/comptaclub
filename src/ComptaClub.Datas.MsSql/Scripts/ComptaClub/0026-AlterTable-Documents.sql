if Col_Length('Documents','CreationDate') is null
Begin
	alter table [Documents] add CreationDate int not null
End
Go

if Col_Length('Documents','LastUpdate') is null
Begin
	alter table [Documents] add LastUpdate int not null
End
Go

if Col_Length('Documents','FileName') is null
Begin
	alter table [Documents] add FileName varchar(1024) not null
End
Go

if Col_Length('Documents','Description') is null
Begin
	alter table [Documents] add Description varchar(2048) null
End
Go

if Col_Length('Documents','MimeType') is null
Begin
	alter table [Documents] add MimeType varchar(100) null
End
Go

if Col_Length('Documents','Size') is null
Begin
	alter table [Documents] add Size bigint not null
End
Go

if Col_Length('Documents','UserOwnerId') is null
Begin
	alter table [Documents] add UserOwnerId uniqueidentifier null
End
Go