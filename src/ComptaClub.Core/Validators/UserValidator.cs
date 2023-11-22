namespace ComptaClub.Validators;

internal class UserValidator : FluentValidation.AbstractValidator<Datas.UserData>
{
	public UserValidator(MediatR.IMediator mediator)
	{
		RuleFor(i => i.Id).ValidGuid();
		RuleFor(i => i.Name).NotEmpty().WithMessage("Ce nom n'est pas valide");
		RuleFor(i => i.Name).NotNull().WithMessage("Le nom doit etre renseigné");
		RuleFor(i => i.Email).EmailAddress().WithMessage("Adresse email invalide");
	}
}
