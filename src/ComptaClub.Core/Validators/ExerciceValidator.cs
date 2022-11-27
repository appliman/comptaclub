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

            // TODO : Verifier qu'il n'exite pas un exercice qui couvre déjà l'interval

            // TODO :
        }
    }
}
