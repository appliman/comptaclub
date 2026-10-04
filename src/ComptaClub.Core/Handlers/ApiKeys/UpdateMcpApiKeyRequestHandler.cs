using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class UpdateMcpApiKeyRequestHandler(McpApiKeyService service) : IRequestHandler<UpdateMcpApiKeyRequest, McpApiKeyResult>
{
    public Task<McpApiKeyResult> Handle(UpdateMcpApiKeyRequest request, CancellationToken cancellationToken) =>
        service.Update(request, cancellationToken);
}
