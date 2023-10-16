if Col_Length('Exercices','ExerciceState') is null
Begin
	alter table [Exercices] add [ExerciceState] [int] null
End
Go

update [Exercices] set [ExerciceState] = 0 where [ExerciceState] is null
Go

alter table [Exercices] alter column [ExerciceState] [int] not null
Go
