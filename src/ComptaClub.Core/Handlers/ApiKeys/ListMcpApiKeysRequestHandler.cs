using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class ListMcpApiKeysRequestHandler(McpApiKeyService service) : IRequestHandler<ListMcpApiKeysRequest, ComptaClub.Contracts.Models.PagedList<IEnumerable<McpApiKeyInfo>>>
{
    public Task<ComptaClub.Contracts.Models.PagedList<IEnumerable<McpApiKeyInfo>>> Handle(ListMcpApiKeysRequest request, CancellationToken cancellationToken) =>
        service.List(request, cancellationToken);
}
