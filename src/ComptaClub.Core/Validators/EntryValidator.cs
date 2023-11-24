using ComptaClub.Configuration;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Validators;

internal class EntryValidator : FluentValidation.AbstractValidator<Datas.EntryData>
{
	public EntryValidator(MediatR.IMediator mediator)
	{
		RuleFor(i => i.Id).ValidGuid();
		RuleFor(i => i.AccountId).ValidGuid();
		RuleFor(i => i.ExerciceId).ValidGuid();
		RuleFor(i => i.BankId).ValidGuid();
		RuleFor(i => i.AccountId).CustomAsync(async (accountId, ctx, cancel) =>
		{
			// Compte d'import
			if (accountId == ComptaClubSettings.ImportAccount)
			{
				return;
			}
			var plan = await mediator.Send(new GetPlanRequest());
			var leafList = plan.GetLeafList();
			if (!leafList.Any(i => i.Id == accountId))
			{
				ctx.AddFailure(nameof(Datas.EntryData.AccountId), "Il n'est pas possible d'associer un compte général à une ecriture");
			}
			else
			{
				var account = leafList.Single(i => i.Id == accountId);
				if (ctx.InstanceToValidate.AccountDirection != account.Direction)
				{
					ctx.AddFailure(nameof(Datas.EntryData.AccountDirection), "Il n'est pas possible d'associer un sens d'ecriture different du compte associé");
				}
			}

		});
		RuleFor(i => i.ExerciceId).CustomAsync(async (exerciceId, ctx, cancel) =>
		{
			var exerciceList = await mediator.Send(new GetAllExercicesRequest());
			if (!exerciceList.Any(i => i.Id == exerciceId))
			{
				ctx.AddFailure(nameof(Datas.EntryData.ExerciceId), "Cet écriture ne peut pas etre associée à un exercice inexistant");
			}
			else
			{
				var exercice = exerciceList.Single(i => i.Id == exerciceId);
				if (exercice.ClosedDate.HasValue
					|| exercice.ExerciceState == Enums.ExerciceState.Closed)
				{
					ctx.AddFailure(nameof(Datas.EntryData.ExerciceId), "Il n'est pas possible de modifier une écriture sur un exercice déjà clos");
				}
				if (!exercice.Active)
				{
					ctx.AddFailure(new FluentValidation.Results.ValidationFailure
					{
						Severity = Severity.Warning,
						PropertyName = nameof(Datas.EntryData.ExerciceId),
						ErrorMessage = "Attention cette écriture n'est pas associée à l'exercice en cours"
					});
				}

				if (ctx.InstanceToValidate.CreationDate < exercice.StartDate)
				{
					ctx.AddFailure(nameof(Datas.EntryData.CreationDate), "La date de creation de l'ecriture doit correspondre à l'interval de date de l'exercice");
				}

				if (ctx.InstanceToValidate.ValueDate < exercice.StartDate)
				{
					ctx.AddFailure(nameof(Datas.EntryData.CreationDate), "La date de valeur de l'ecriture doit correspondre à l'interval de date de l'exercice");
				}

				if (ctx.InstanceToValidate.CreationDate > exercice.EndDate)
				{
					ctx.AddFailure(nameof(Datas.EntryData.CreationDate), "La date de creation de l'ecriture doit correspondre à l'interval de date de l'exercice");
				}

				if (ctx.InstanceToValidate.ValueDate > exercice.EndDate)
				{
					ctx.AddFailure(nameof(Datas.EntryData.CreationDate), "La date de valeur de l'ecriture doit correspondre à l'interval de date de l'exercice");
				}
			}
		});
		RuleFor(i => i.PartNumber).NotNull().NotEmpty().WithMessage("Une écriture doit comporter un numéro de pièce");
		RuleFor(i => i.Label).NotNull().NotEmpty().WithMessage("Une écriture doit comporter un libellé");
		RuleFor(i => i.UserCreatorId).NotEmpty();
	}
}
