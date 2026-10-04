using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class RevokeMcpApiKeyRequestHandler(McpApiKeyService service) : IRequestHandler<RevokeMcpApiKeyRequest, McpApiKeyResult>
{
    public Task<McpApiKeyResult> Handle(RevokeMcpApiKeyRequest request, CancellationToken cancellationToken) =>
        service.Revoke(request.Id, false, cancellationToken);
}
