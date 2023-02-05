namespace ComptaClub.Validators;

internal class AssociatedMemberListByEntryValidator : FluentValidation.AbstractValidator<Datas.AssociatedMemberListByEntryData>
{
	public AssociatedMemberListByEntryValidator(IMediator mediator)
	{
		RuleFor(i => i.MemberId).ValidGuid();
		RuleFor(i => i.EntryId).ValidGuid();
		RuleFor(i => i.Amount).GreaterThanOrEqualTo(0).WithMessage("Le montant doit etre supérieur ou égal à zero");
		// Todo
		// Le montant cumulé ne doit pas etre supérieur à l'ecriture
    }
}
