using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace ComptaClub.Validators
{
    public class ExerciceValidator : FluentValidation.AbstractValidator<Models.Exercice>
    {
        public ExerciceValidator(MediatR.IMediator mediator)
        {
            RuleFor(i => i.Id).ValidGuid();
            RuleFor(i => i.Code).CustomAsync(async (code, ctx, cancel) =>
            {
                var current = ctx.InstanceToValidate;
                var existing = await mediator.Send(new Requests.GetExerciceByFilterRequest(f => f.PartitionKey == code));
                if (existing != null 
                    && current.Id != existing.Id)
                {
                    ctx.AddFailure(nameof(Models.Account.Code), "Ce code est déjà utilisé");
                }
            });

            // TODO : Verifier qu'il n'exite pas un exercice qui couvre déjà l'interval

            // TODO :
        }
    }
}
