using FluentValidation;

using MediatR;

namespace ComptaClub.Validators
{
    public class BankValidator : FluentValidation.AbstractValidator<Datas.BankData>
    {
        public BankValidator(IMediator mediator)
        {
            RuleFor(i => i.Id).ValidGuid();
            RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await mediator.Send(new Requests.GetBankByFilterRequest(i => i.Code == code));
                if (existing != null && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Datas.BankData.Code), "Ce nom de banque est déjà utilisé");
                }
            });
        }
    }
}
