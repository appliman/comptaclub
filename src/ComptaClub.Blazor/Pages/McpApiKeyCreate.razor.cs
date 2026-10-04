using ChannelMediator;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.ApiKeys;

namespace ComptaClub.Blazor.Pages;

public partial class McpApiKeyCreate
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    private readonly ApiKeyFormModel _model = new();
    private string? _secret;
    private string? _error;
    private bool _busy;

    private async Task Create()
    {
        if (_busy || _secret is not null)
        {
            return;
        }
        _busy = true;
        _error = null;
        try
        {
            var _result = await Mediator.Send(new CreateMcpApiKeyRequest(_model.Name, _model.GetExpiration()));
            _error = _result.HasError ? _result.GetAllBrokenRules() : null;
            _secret = _result.PlainTextKey;
        }
        catch (Exception _exception) when (_exception is ArgumentException or UnauthorizedAccessException)
        {
            _error = _exception.Message;
        }
        finally
        {
            _busy = false;
        }
    }

    private void Finish()
    {
        _secret = null;
        Navigation.NavigateTo("/configuration/cles-api");
    }
}
