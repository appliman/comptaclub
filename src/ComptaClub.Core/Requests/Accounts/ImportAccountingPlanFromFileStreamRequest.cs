using ComptaClub.Results;

namespace ComptaClub.Requests.Accounts;

public record ImportAccountingPlanFromFileStreamRequest : IRequest<CommandResult>
{
    public ImportAccountingPlanFromFileStreamRequest(MemoryStream contentStream)
    {
        ContentStream = contentStream;
    }

    public MemoryStream ContentStream { get; init; }
}
