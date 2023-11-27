namespace ComptaClub.Datas.Enums;
public enum MemberState
{
    [Display(Name = "Actif", Description = "Membre actif pour la saison en cours")]
    Active = 1,
    [Display(Name = "Inactif", Description = "Membre non actif pour la saison en cours")]
    Disabled = 2
}
