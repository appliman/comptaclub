using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class GetMcpApiKeyRequestHandler(McpApiKeyService service) : IRequestHandler<GetMcpApiKeyRequest, McpApiKeyInfo?>
{
    public Task<McpApiKeyInfo?> Handle(GetMcpApiKeyRequest request, CancellationToken cancellationToken) =>
        service.Get(request.Id, cancellationToken);
}
