using ComptaClub.Services;

using FluentValidation;

namespace ComptaClub.Validators
{
    public class AccountValidator : FluentValidation.AbstractValidator<Models.Account>
    {
        public AccountValidator(AccountingService accountingService)
        {
            RuleFor(i => i.Id).Custom((id, ctx) =>
            {
                if (id == Guid.Empty)
                {
                    ctx.AddFailure(nameof(Models.Account.Id), "Identifiant invalide");
                }
            });
            RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await accountingService.GetAccountByCode(code);
                if (existing != null && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Models.Account.Code), "Ce code est déjà utilisé");
                }
            });
            RuleFor(i => i.Label).NotNull().NotEmpty().WithMessage("Un compte doit avoir un libellé");
        }
    }
}
