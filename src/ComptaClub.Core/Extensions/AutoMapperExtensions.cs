using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Extensions
{
    public static class AutoMapperCompatibilityExtensions
    {
        public static void ResolveUsing<TSource, TDestination, TMember, TResult>(this IMemberConfigurationExpression<TSource, TDestination, TMember> member,
                                Func<TSource, TResult> resolver)
            => member.MapFrom((src, dest) => resolver(src));
    }
}
