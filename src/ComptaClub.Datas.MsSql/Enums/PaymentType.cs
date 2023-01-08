namespace ComptaClub.Datas;

public enum PaymentType
{
	[Display(Name = "Virement", Description = "Paiement par virement")]
	Transfer = 1,
	[Display(Name = "Chèque", Description = "Paiement par chèque")]
	Check = 2,
	[Display(Name = "Espèce", Description = "Paiement par espèce")]
	Cash = 3,
	[Display(Name = "En ligne", Description = "Paiement en ligne")]
	Online = 4,
	[Display(Name = "Carte bancaire", Description = "Paiement par carte bancaire")]
	CreditCard = 5,
    [Display(Name = "Prélèvement", Description = "Paiement par prélèvement")]
    Debit = 6,
    [Display(Name = "Import", Description = "Donnée importée")]
    Import = 7,
}
