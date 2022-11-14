using FluentValidation;

namespace ComptaClub.Validators
{
    public class AccountValidator : FluentValidation.AbstractValidator<Models.Account>
    {
        public AccountValidator(MediatR.IMediator mediator)
        {
            RuleFor(i => i.Id).ValidGuid();
            RuleFor(i => i.ParentAccountId).ValidGuid();
            RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await mediator.Send(new  Requests.GetAccountByCode(code));
                if (existing != null && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Models.Account.Code), "Ce code est déjà utilisé");
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
                var directions = Enum.GetValues<Datas.AccountDirection>();
                if (!directions.Any(i => i == direction))
                {
                    ctx.AddFailure(nameof(Models.Account.Direction), "Le sens doit etre indiqué");
                }
            });
            // TODO : Verifier que le parent existe
            // TODO : Verifier que le code du parent commence bien par les meme codes
        }
    }
}
