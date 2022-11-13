using System.IO.Pipes;

using ComptaClub.Services;

using FluentValidation;

namespace ComptaClub.Validators
{
    public class AccountValidator : FluentValidation.AbstractValidator<Models.Account>
    {
        public AccountValidator(MediatR.IMediator mediator)
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
                var existing = await mediator.Send(new  Requests.GetAccountByCode(code));
                if (existing != null && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Models.Account.Code), "Ce code est déjà utilisé");
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
        }
    }
}
