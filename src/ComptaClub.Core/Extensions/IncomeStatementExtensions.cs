using ComptaClub.Contracts.Models;
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
