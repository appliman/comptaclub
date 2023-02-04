
namespace ComptaClub.Requests;

public record ImportAccountingPlanRequest : IRequest<Results.CommandResult>
{
	public ImportAccountingPlanRequest(List<Datas.AccountData> plan)
	{
		this.HierarchizedAccountingPlan = plan;
	}

	public List<Datas.AccountData> HierarchizedAccountingPlan { get; init; }
}
