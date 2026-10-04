using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class CreateMcpApiKeyRequestHandler(McpApiKeyService service) : IRequestHandler<CreateMcpApiKeyRequest, McpApiKeyResult>
{
    public Task<McpApiKeyResult> Handle(CreateMcpApiKeyRequest request, CancellationToken cancellationToken) =>
        service.Create(request, cancellationToken);
}
