using ComptaClub.Contracts.Models.Members;

using MediatR;

namespace ComptaClub.Validators;

internal class MemberValidator : FluentValidation.AbstractValidator<Datas.MemberData>
{
	public MemberValidator(MediatR.IMediator mediator)
	{
        RuleFor(i => i.Id).ValidGuid();
        RuleFor(i => i.Email).EmailAddress().WithMessage("L'adresse email indiquée n'est pas valide");
        RuleFor(i => i.Name).NotNull().NotEmpty().WithMessage("Le nom du membre doit etre indiqué");
        RuleFor(i => i.LicenseNumber).CustomAsync(async (licenseNumber, ctx, cancel) =>
        {
            var current = ctx.InstanceToValidate;
            var existing = await mediator.Send(new GetMemberByFilterRequest(i => i.LicenseNumber = licenseNumber));
            if (existing != null && current.Id != existing.Id)
            {
                ctx.AddFailure(nameof(Datas.AccountData.Code), "Ce membre exist déjà");
            }
        });

    }

}
