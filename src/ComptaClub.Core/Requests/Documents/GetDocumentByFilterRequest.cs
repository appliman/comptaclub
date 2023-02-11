using System.Linq.Expressions;

namespace ComptaClub.Requests.Documents;

public record GetDocumentByFilterRequest : IRequest<DocumentData?>
{
    public GetDocumentByFilterRequest(Action<DocumentListFilter> filter)
    {
        var defaultFilter = new DocumentListFilter();
        filter(defaultFilter);
        Filter = filter;
    }

    public Action<DocumentListFilter> Filter { get; init; }
}
