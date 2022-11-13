using ComptaClub.Services;

using FluentValidation;

namespace ComptaClub.Validators
{
    public class BankValidator : FluentValidation.AbstractValidator<Models.Bank>
    {
        public BankValidator(Services.AccountingService accountingService)
        {
            RuleFor(i => i.Id).Custom((id, ctx) =>
            {
                if (id == Guid.Empty)
                {
                    ctx.AddFailure(nameof(Models.Account.Id), "Identifiant invalide");
                }
            });
            RuleFor(i => i.Name).CustomAsync(async (name, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await accountingService.GetBankByName(name);
                if (existing != null && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Models.Account.Code), "Ce nom de banque est déjà utilisé");
                }
            });
        }
    }
}
