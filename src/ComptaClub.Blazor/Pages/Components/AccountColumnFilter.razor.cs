namespace ComptaClub.Blazor.Pages.Components;

public partial class AccountColumnFilter : ComponentBase
{
    [Parameter]
    public IReadOnlyList<ViewModels.Account> Accounts { get; set; } = [];

    [Parameter]
    public List<Guid>? Value { get; set; }

    [Parameter]
    public EventCallback<List<Guid>?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback ApplyFilter { get; set; }

    private readonly HashSet<Guid> _selectedIds = [];
    private bool _isOpen;
    private string _search = string.Empty;

    private string TriggerLabel => Value?.Count switch
    {
        null or 0 => "Tous les comptes",
        1 => Accounts.FirstOrDefault(account => account.Id == Value[0])?.CodeAndLabel ?? "1 compte",
        _ => $"{Value.Count} comptes"
    };

    private IEnumerable<ViewModels.Account> FilteredAccounts => Accounts.Where(account =>
        string.IsNullOrWhiteSpace(_search) ||
        account.CodeAndLabel.Contains(_search, StringComparison.CurrentCultureIgnoreCase));

    protected override void OnParametersSet()
    {
        if (!_isOpen)
        {
            _selectedIds.Clear();
            _selectedIds.UnionWith(Value ?? []);
        }
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        if (_isOpen)
        {
            _search = string.Empty;
            _selectedIds.Clear();
            _selectedIds.UnionWith(Value ?? []);
        }
    }

    private void SetSelected(Guid accountId, bool selected)
    {
        if (selected)
        {
            _selectedIds.Add(accountId);
        }
        else
        {
            _selectedIds.Remove(accountId);
        }
    }

    private async Task Clear()
    {
        _selectedIds.Clear();
        await Apply();
    }

    private async Task Apply()
    {
        var selectedIds = Accounts.Where(account => _selectedIds.Contains(account.Id))
            .Select(account => account.Id).ToList();
        _isOpen = false;
        await ValueChanged.InvokeAsync(selectedIds);
        await ApplyFilter.InvokeAsync();
    }
}
