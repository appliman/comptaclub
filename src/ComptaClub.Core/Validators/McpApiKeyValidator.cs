namespace ComptaClub.Validators;

public sealed class McpApiKeyValidator : AbstractValidator<McpApiKeyData>
{
    public McpApiKeyValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.CreatedByUserId).NotEmpty();
    }
}
