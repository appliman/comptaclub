using ComptaClub.Contracts.Models.Accounts;

namespace ComptaClub.Validators;

internal class AccountValidator : FluentValidation.AbstractValidator<Datas.AccountData>
{
    public AccountValidator(ChannelMediator.IMediator mediator, IComptaClubDbContextFactory factory)
    {
        RuleFor(i => i.Id).ValidGuid();
        RuleFor(i => i.ParentAccountId).ValidGuid();
        RuleFor(i => i.ParentAccountId).CustomAsync(async (parentId, context, cancellationToken) =>
        {
            await using var _db = await factory.CreateDbContextAsync(cancellationToken);
            var _visited = new HashSet<Guid> { context.InstanceToValidate.Id };
            var _parentId = parentId;
            while (_parentId.HasValue)
            {
                if (!_visited.Add(_parentId.Value))
                {
                    context.AddFailure("La hiérarchie des comptes ne peut pas contenir de cycle.");
                    return;
                }
                var _parent = await _db.Accounts.SingleOrDefaultAsync(item => item.Id == _parentId.Value, cancellationToken);
                if (_parent is null)
                {
                    context.AddFailure("Le compte parent est introuvable.");
                    return;
                }
                _parentId = _parent.ParentAccountId;
            }
        });
        RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
        {
            var current = ctx.InstanceToValidate;
            var existing = await mediator.Send(new  GetAccountByFilterRequest(i => i.Code = code), cancel);
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
        // TODO : Verifier que le code du parent commence bien par les meme codes
    }
}
