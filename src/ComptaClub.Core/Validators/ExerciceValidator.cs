using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace ComptaClub.Validators
{
    public class ExerciceValidator : FluentValidation.AbstractValidator<Datas.ExerciceData>
    {
        public ExerciceValidator(MediatR.IMediator mediator)
        {
            RuleFor(i => i.Id).ValidGuid();
            RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await mediator.Send(new Requests.GetExerciceByFilterRequest(f => f.Code == code));
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
			RuleFor(i => i.StartDate).Custom((startDate, ctx) =>
			{
				if (startDate < 0)
				{
					ctx.AddFailure("Vous devez indiquer une date à partir de 2000");
				}
				var s = startDate.FromDayId();
				if (startDate >= ctx.InstanceToValidate.EndDate)
				{
					ctx.AddFailure("La date de début d'exercice doit etre supérieure à la date de fin");
				}
			});

			// TODO : Verifier qu'il n'exite pas un exercice qui couvre déjà l'interval
		}
	}
}
