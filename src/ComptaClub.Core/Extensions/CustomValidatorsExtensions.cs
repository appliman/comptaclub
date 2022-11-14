using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;
using FluentValidation.Validators;

namespace ComptaClub.Extensions;

public static class CustomValidatorExtensions
{
    public static IRuleBuilderOptions<T, Guid> ValidGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder.Must(g => g != Guid.Empty).WithMessage("Cet identifiant est incorrect");
    }

    public static IRuleBuilderOptions<T, Guid?> ValidGuid<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder.Must(g => g == null || g != Guid.Empty).WithMessage("Cet identifiant est incorrect");
    }

}
