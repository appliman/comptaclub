namespace ComptaClub.Requests.Accounts;

public record ImportAccountingPlanRequest : IRequest<Results.CommandResult>
{
    public ImportAccountingPlanRequest(List<AccountData> plan)
    {
        HierarchizedAccountingPlan = plan;
    }

    public List<AccountData> HierarchizedAccountingPlan { get; init; }
}
