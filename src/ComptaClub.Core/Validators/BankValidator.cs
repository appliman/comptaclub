using ComptaClub.Services;

using FluentValidation;

using MediatR;

namespace ComptaClub.Validators
{
    public class BankValidator : FluentValidation.AbstractValidator<Models.Bank>
    {
        public BankValidator(IMediator mediator)
        {
            RuleFor(i => i.Id).ValidGuid();
            RuleFor(i => i.Code).CustomAsync(async (name, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await mediator.Send(new Requests.GetBankByCodeRequest(name));
                if (existing != null && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Models.Account.Code), "Ce nom de banque est déjà utilisé");
                }
            });
        }
    }
}
