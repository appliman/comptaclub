using System.Linq.Expressions;

namespace ComptaClub.Contracts.Models.Documents;

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
