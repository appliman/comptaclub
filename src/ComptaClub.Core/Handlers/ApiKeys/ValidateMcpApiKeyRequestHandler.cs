using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;

namespace ComptaClub.Handlers.ApiKeys;

internal sealed class ValidateMcpApiKeyRequestHandler(McpApiKeyService service) : IRequestHandler<ValidateMcpApiKeyRequest, McpApiKeyData?>
{
    public Task<McpApiKeyData?> Handle(ValidateMcpApiKeyRequest request, CancellationToken cancellationToken) =>
        service.Validate(request.PlainTextKey, cancellationToken);
}
