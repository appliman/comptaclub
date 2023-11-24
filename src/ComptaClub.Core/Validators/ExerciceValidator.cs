using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Validators;

internal class ExerciceValidator : FluentValidation.AbstractValidator<Datas.ExerciceData>
{
	public ExerciceValidator(MediatR.IMediator mediator)
	{
		RuleFor(i => i.Id).ValidGuid();
		RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
		{
			var current = ctx.InstanceToValidate;
			var existing = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == code));
			if (existing != null
				&& current.Id != existing.Id)
			{
				ctx.AddFailure(nameof(Datas.ExerciceData.Code), "Ce code est déjà utilisé");
			}
		});
		RuleFor(i => i.Code).Custom((code, ctx) =>
		{
			if (code != null
				&& code.Equals("a completer", StringComparison.InvariantCultureIgnoreCase))
			{
				ctx.AddFailure(nameof(Datas.ExerciceData.Code), "Vous devez indiquer un code valide");
			}
		});
		RuleFor(i => i.Label).Custom((label, ctx) =>
		{
			if (label != null
				&& label.Equals("a completer", StringComparison.InvariantCultureIgnoreCase))
			{
				ctx.AddFailure(nameof(Datas.ExerciceData.Label), "Vous devez indiquer un libellé valide");
			}
		});
		RuleFor(i => i.StartDate).Custom((startDate, ctx) =>
		{
			if (startDate < 0)
			{
				ctx.AddFailure(nameof(Datas.ExerciceData.StartDate), "Vous devez indiquer une date à partir de 2000");
			}
			if (startDate >= ctx.InstanceToValidate.EndDate)
			{
				ctx.AddFailure(nameof(Datas.ExerciceData.StartDate), "La date de début d'exercice doit etre supérieure à la date de fin");
			}
		});
		RuleFor(i => i.Active).CustomAsync(async (active, ctx, cancel) =>
		{
			if (active)
			{
				var alreadyActiveExercice = await mediator.Send(new GetActiveExerciceRequest());
				if (alreadyActiveExercice != null
					&& alreadyActiveExercice.Id != ctx.InstanceToValidate.Id)
				{
					ctx.AddFailure(nameof(Datas.ExerciceData.Active), "Vous ne pouvez pas avoir 2 exercices actifs en cours");
				}
			}
		});

		// TODO : Verifier qu'il n'exite pas un exercice qui couvre déjà l'interval
	}
}

