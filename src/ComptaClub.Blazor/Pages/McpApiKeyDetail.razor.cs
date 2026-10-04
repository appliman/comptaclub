using ChannelMediator;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.ApiKeys;

namespace ComptaClub.Blazor.Pages;

public partial class McpApiKeyDetail
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private DialogService Dialogs { get; set; } = default!;
    private ApiKeyFormModel _model = new();
    private McpApiKeyInfo? _key;
    private string? _secret;
    private string? _error;
    private string? _message;
    private bool _busy;

    protected override async Task OnParametersSetAsync()
    {
        _secret = null;
        await Load();
    }

    private async Task Load()
    {
        try
        {
            _key = await Mediator.Send(new GetMcpApiKeyRequest(Id));
            if (_key is null)
            {
                _error = "Clé API introuvable.";
                return;
            }
            _model = new ApiKeyFormModel
            {
                Name = _key.Name, ExpirationUtc = _key.ExpirationDateUtc?.ToString("yyyy-MM-ddTHH:mm") ?? string.Empty
            };
        }
        catch (UnauthorizedAccessException _exception)
        {
            _error = _exception.Message;
        }
    }

    private Task Save() => Execute(() => Mediator.Send(new UpdateMcpApiKeyRequest(Id, _key!.Version, _model.Name, _model.GetExpiration())), "La clé a été mise à jour.");

    private async Task Rotate()
    {
        if (await Dialogs.Confirm("Renouveler la clé", "L’ancien secret sera immédiatement invalidé.", new ConfirmOptions { OkButtonText = "Renouveler", CancelButtonText = "Annuler" }))
        {
            await Execute(() => Mediator.Send(new RotateMcpApiKeyRequest(Id, _key!.Version)), "Copiez le nouveau secret.");
        }
    }

    private async Task Revoke()
    {
        if (await Dialogs.Confirm("Révoquer la clé", "Les prochains appels utilisant cette clé seront refusés.", new ConfirmOptions { OkButtonText = "Révoquer", CancelButtonText = "Annuler" }))
        {
            await Execute(() => Mediator.Send(new RevokeMcpApiKeyRequest(Id)), "La clé a été révoquée.");
        }
    }

    private async Task Archive()
    {
        if (await Dialogs.Confirm("Archiver la clé", "La clé sera révoquée et retirée de la liste.", new ConfirmOptions { OkButtonText = "Archiver", CancelButtonText = "Annuler" }))
        {
            await Execute(() => Mediator.Send(new ArchiveMcpApiKeyRequest(Id)), "La clé a été archivée.");
        }
    }

    private async Task Execute(Func<Task<McpApiKeyResult>> action, string message)
    {
        if (_busy || _key is null)
        {
            return;
        }
        _busy = true;
        _error = null;
        _message = null;
        try
        {
            var _result = await action();
            _error = _result.HasError ? _result.GetAllBrokenRules() : null;
            if (!_result.HasError)
            {
                _secret = _result.PlainTextKey;
                _message = message;
            }
            await Load();
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
}
