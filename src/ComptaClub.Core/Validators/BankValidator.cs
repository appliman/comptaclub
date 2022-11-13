using ComptaClub.Services;

using FluentValidation;

using MediatR;

namespace ComptaClub.Validators
{
    public class BankValidator : FluentValidation.AbstractValidator<Models.Bank>
    {
        public BankValidator(IMediator mediator)
        {
            RuleFor(i => i.Id).Custom((id, ctx) =>
            {
                if (id == Guid.Empty)
                {
                    ctx.AddFailure(nameof(Models.Account.Id), "Identifiant invalide");
                }
            });
            RuleFor(i => i.Code).CustomAsync(async (name, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await mediator.Send(new Requests.GetBankByCode(name));
                if (existing != null && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Models.Account.Code), "Ce nom de banque est déjà utilisé");
                }
            });
        }
    }
}
