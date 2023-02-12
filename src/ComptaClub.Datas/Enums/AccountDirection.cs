namespace ComptaClub.Enums
{
    public enum AccountDirection
    {
        [Display(Name = "Débit", Description = "Débit")]
        Debit = -1,
        [Display(Name = "Import", Description = "Import")]
        Import = 0,
        [Display(Name = "Crédit", Description = "Crédit")]
        Credit = 1,

    }
}
