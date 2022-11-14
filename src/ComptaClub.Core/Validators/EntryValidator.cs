using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace ComptaClub.Validators
{
    public class EntryValidator : FluentValidation.AbstractValidator<Models.Entry>
    {
        public EntryValidator()
        {
            RuleFor(i => i.Id).ValidGuid();
            RuleFor(i => i.AccountId).ValidGuid();
            RuleFor(i => i.ExerciceId).ValidGuid();
            RuleFor(i => i.BankId).ValidGuid();
        }
    }
}
