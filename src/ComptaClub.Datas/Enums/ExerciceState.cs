namespace ComptaClub.Enums;

public enum ExerciceState
{
    [Display(Name = "En cours", Description = "Exercice en cours")]
    Current = 0,
    [Display(Name = "Cloturé", Description = "Exercice cloturé")]
    Closed = 1
}
