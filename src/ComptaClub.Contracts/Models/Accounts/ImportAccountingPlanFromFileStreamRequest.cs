using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Accounts;

public record ImportAccountingPlanFromFileStreamRequest : IRequest<CommandResult>
{
    public ImportAccountingPlanFromFileStreamRequest(MemoryStream contentStream)
    {
        ContentStream = contentStream;
    }

    public MemoryStream ContentStream { get; init; }
}
