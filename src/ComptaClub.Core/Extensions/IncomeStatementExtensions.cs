using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Models.IncomeStatements;

namespace ComptaClub.Extensions;
public static class IncomeStatementExtensions
{
    public static async Task<IncomeStatementData?> GetIncomeStatementDataById(this IMediator mediator, Guid incomeStatementId)
    {
        var filter = new IncomeStatementListFilter();
        filter.IdList.Add(incomeStatementId);

        var incomeStatementList = await mediator.Send(new GetPagedEntityListRequest<IncomeStatementListFilter, IncomeStatementData>(filter));
        return incomeStatementList.List.SingleOrDefault();
    }
}
