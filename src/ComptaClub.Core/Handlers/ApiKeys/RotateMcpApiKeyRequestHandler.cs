using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class RotateMcpApiKeyRequestHandler(McpApiKeyService service) : IRequestHandler<RotateMcpApiKeyRequest, McpApiKeyResult>
{
    public Task<McpApiKeyResult> Handle(RotateMcpApiKeyRequest request, CancellationToken cancellationToken) =>
        service.Rotate(request, cancellationToken);
}
