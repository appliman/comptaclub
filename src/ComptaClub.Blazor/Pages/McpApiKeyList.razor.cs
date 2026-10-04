using ChannelMediator;
using ComptaClub.Contracts.Models.ApiKeys;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class McpApiKeyList
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    private SuperDataGrid<McpApiKeyInfo>? _grid;
    private string? _error;

    private async ValueTask<GridItemsProviderResult<McpApiKeyInfo>> LoadItems(GridItemsProviderRequest<McpApiKeyInfo> request)
    {
        try
        {
            var _count = Math.Clamp(request.Count ?? 50, 1, 100);
            var _start = Math.Max(0, request.StartIndex);
            var _pageIndex = _start / 100;
            var _result = await Mediator.Send(new ListMcpApiKeysRequest(_pageIndex, 100), request.CancellationToken);
            var _items = _result.List.Skip(_start % 100).ToList();
            if (_items.Count < _count && (_pageIndex + 1) * 100 < _result.Total.RowCount)
            {
                var _next = await Mediator.Send(new ListMcpApiKeysRequest(_pageIndex + 1, 100), request.CancellationToken);
                _items.AddRange(_next.List);
            }
            return GridItemsProviderResult<McpApiKeyInfo>.From(_items.Take(_count).ToList(), _result.Total.RowCount);
        }
        catch (UnauthorizedAccessException)
        {
            _error = "Votre compte doit être actif pour gérer les clés API.";
            return GridItemsProviderResult<McpApiKeyInfo>.From([], 0);
        }
    }

    internal static string Status(McpApiKeyInfo key) =>
        key.ArchivedDateUtc.HasValue ? "Archivée" : key.RevokedDateUtc.HasValue ? "Révoquée" :
        key.ExpirationDateUtc <= DateTime.UtcNow ? "Expirée" : "Active";
}
