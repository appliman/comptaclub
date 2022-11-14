using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ComptaClub.Datas
{
    public enum AccountDirection
    {
        [Display(Name = "Crédit", Description = "Crédit")]
        Credit = 1,
		[Display(Name = "Débit", Description = "Débit")]
		Debit = -1
    }
}
