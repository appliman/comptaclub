using FluentValidation;

using MediatR;

namespace ComptaClub.Validators;

internal class BankValidator : FluentValidation.AbstractValidator<Datas.BankData>
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

    }
}
