using ComptaClub.Results;

namespace ComptaClub.Requests;

public record ImportAccountingPlanFromFileStreamRequest : IRequest<CommandResult>
{
    public ImportAccountingPlanFromFileStreamRequest(MemoryStream contentStream)
    {
        this.ContentStream = contentStream;
    }

    public MemoryStream ContentStream { get; init; }
}
