

create database ComptaClubDev
Go

Create login ComptaClubDevUsr 
With Password = '4*0S7hMQt#b^c',
	CHECK_EXPIRATION = OFF,
	DEFAULT_DATABASE = ComptaClubDev

Go

USE [ComptaClubDev]
GO

CREATE USER [ComptaClubDevUsr] FOR LOGIN [ComptaClubDevUsr]
GO

ALTER ROLE [db_owner] ADD MEMBER [ComptaClubDevUsr]
GO