using ComptaClub.Blazor.ViewModels;

using ChannelMediator;

namespace ComptaClub.Blazor.Pages.Components;

public partial class AccountLinkSelector
{
    [Parameter]
    public Guid AccountId { get; set; }

    [Parameter]
    public EventCallback<Guid> AccountIdChanged { get; set; }

    [Parameter]
    public System.Linq.Expressions.Expression<Func<Guid>>? AccountIdExpression { get; set; }

    [Parameter]
    public AccountDirection AccountDirection { get; set; } = AccountDirection.Import;

    [Inject]
    IMediator Mediator { get; set; } = default!;

    [Inject]
    DialogService DialogService { get; set; } = default!;

    string accountTitle = "Selectionner";
    protected override async Task OnInitializedAsync()
    {
        var account = await Mediator.GetAccountById(AccountId);
        if (account is not null)
        {
            accountTitle = $"{account.Code} {account.Label}";
        }
    }

    async Task OpenAccountSelectorDialog()
    {
        var title = "Selection d'un compte";
        if (AccountDirection == AccountDirection.Debit)
        {
            title += " de Débit";
        }
        else if (AccountDirection == AccountDirection.Credit)
        {
            title += " de Crédit";
        }
        var dialog = await DialogService.OpenAsync<Dialogs.AccountSelectorDialog>(title,
            parameters: new Dictionary<string, object>
            {
                { "AccountDirection", AccountDirection },
                { "AccountId", AccountId }
            },
            options: new DialogOptions
            {

                Height = "600px",
            });

        if (dialog is ViewModels.Account selectedAccount)
        {
            AccountId = selectedAccount.Id;
            accountTitle = $"{selectedAccount.Code} {selectedAccount.Label}";
            if (AccountIdChanged.HasDelegate)
            {
                await AccountIdChanged.InvokeAsync(AccountId);
            }
        }

    }
}
