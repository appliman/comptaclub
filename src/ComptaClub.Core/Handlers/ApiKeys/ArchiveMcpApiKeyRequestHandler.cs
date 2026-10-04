using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class ArchiveMcpApiKeyRequestHandler(McpApiKeyService service) : IRequestHandler<ArchiveMcpApiKeyRequest, McpApiKeyResult>
{
    public Task<McpApiKeyResult> Handle(ArchiveMcpApiKeyRequest request, CancellationToken cancellationToken) =>
        service.Revoke(request.Id, true, cancellationToken);
}
