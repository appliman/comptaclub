using ComptaClub.Contracts.Models.Accounts;

namespace ComptaClub.Validators;

internal class AccountValidator : FluentValidation.AbstractValidator<Datas.AccountData>
{
    public AccountValidator(ChannelMediator.IMediator mediator)
    {
        RuleFor(i => i.Id).ValidGuid();
        RuleFor(i => i.ParentAccountId).ValidGuid();
        RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
        {
            var current = ctx.InstanceToValidate;
            var existing = await mediator.Send(new  GetAccountByFilterRequest(i => i.Code = code));
            if (existing != null && current.Id != existing.Id)
            {
                ctx.AddFailure(nameof(Datas.AccountData.Code), "Ce code est déjà utilisé");
            }
        });
        RuleFor(i => i.Code).Custom((code, ctx) =>
        {
            if (code != null
                && code.Equals("a completer", StringComparison.InvariantCultureIgnoreCase))
            {
                ctx.AddFailure("Vous devez indiquer un code valide");      
            }
        });
		RuleFor(i => i.Label).Custom((label, ctx) =>
		{
			if (label != null
				&& label.Equals("a completer", StringComparison.InvariantCultureIgnoreCase))
			{
				ctx.AddFailure("Vous devez indiquer un libellé valide");
			}
		});
		RuleFor(i => i.Label).NotNull().NotEmpty().WithMessage("Un compte doit avoir un libellé");
        RuleFor(i => i.Direction).Custom((direction, ctx) =>
        {
            var directions = Enum.GetValues<Enums.AccountDirection>();
            if (!directions.Any(i => i == direction))
            {
                ctx.AddFailure(nameof(Datas.AccountData.Direction), "Le sens doit etre indiqué");
            }
        });
        // TODO : Verifier que le parent existe
        // TODO : Verifier que le code du parent commence bien par les meme codes
    }
}
