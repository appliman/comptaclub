using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Results;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class AccountingPlan : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;


	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;


	IEnumerable<ViewModels.Account> accountList = new List<ViewModels.Account>();
	SuperDataGrid<ViewModels.Account>? grid;
	ViewModels.Account? accountToUpdate;
	ViewModels.Account? accountToInsert;
	List<BrokenRule> brokenRules = new();
	bool uploadEnabled = false;
	string? uploadError = null;
	string? uploadSuccess = null;
	bool isPlanLoaded;

	protected override async Task OnInitializedAsync()
	{
		await LoadDatas();
		isPlanLoaded = true;
	}

	async Task LoadDatas()
	{
		var dataPlan = await Mediator.Send(new GetPlanRequest());
		accountList = Mapping.Profile.ToViewModels(dataPlan);
	}

	ValueTask<GridItemsProviderResult<ViewModels.Account>> LoadItems(GridItemsProviderRequest<ViewModels.Account> request)
	{
		var source = (request.ParentItem?.Children ?? accountList).ToList();
		return ValueTask.FromResult(GridItemsProviderResult<ViewModels.Account>.From(
			source.Skip(request.StartIndex).Take(request.Count ?? source.Count).ToList(), source.Count));
	}

	async Task EditRow(ViewModels.Account account)
	{
		accountToUpdate = account;
		await grid!.BeginEditAsync(account);
	}

	async Task SaveRow(ViewModels.Account account)
	{
		var data = Mapping.Profile.ToData(account);
		var saveResult = await Mediator.Send(new SaveEntityRequest<Datas.AccountData>(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		accountToInsert = null;
		accountToUpdate = null;
		await grid!.EndEditAsync(account);
		await LoadDatas();
		await grid.ReloadAsync();
		brokenRules.Clear();
	}

	async Task CancelEdit(ViewModels.Account account)
	{
		if (account == accountToInsert)
		{
			accountToInsert = null;
		}

		accountToUpdate = null;

		await grid!.CancelEditAsync(account);
		await LoadDatas();
		await grid.ReloadAsync();
	}

	async Task InsertRow()
	{
		var data = await Mediator.Send(new CreateAccountRequest());
		accountToInsert = Mapping.Profile.ToViewModel(data);
		accountList = new[] { accountToInsert }.Concat(accountList).ToList();
		await grid!.ReloadAsync();
		await grid.BeginEditAsync(accountToInsert);
	}

	async Task InsertRow(ViewModels.Account account)
	{
		var data = await Mediator.Send(new CreateAccountRequest()
		{
			Direction = account.Direction,
			ParentId = account.Id
		});
		accountToInsert = Mapping.Profile.ToViewModel(data);
		account.Children.Add(accountToInsert);
		await grid!.ReloadAsync();
		await grid.ExpandAllAsync();
		await grid.BeginEditAsync(accountToInsert);
	}

	async Task DeleteRow(ViewModels.Account account)
	{
		var confirm = await MainLayout.DialogService.Confirm("Suppression", "Confirmez vous la suppression de ce compte");
		if (!confirm)
		{
			return;
		}
		var result = await Mediator!.Send(new DeleteAccountRequest(account.Id));
		if (result.HasError)
		{
			await MainLayout.NotificationService.NotifyError(result);
		}
		else
		{
			await LoadDatas();
			await grid!.ReloadAsync();
			await MainLayout.NotificationService.Notify(new NotificationMessage
			{
				Severity = NotificationSeverity.Info,
				Summary = $"Ce compte ({account.Code}) vient d'être supprimé"
			});
		}
	}

	async Task ExportToJson()
	{
		var fileName = $"{Guid.NewGuid()}.json";
		var path = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var request = new ExportPlanToJsonFileRequest(System.IO.Path.Combine(path, fileName));
		var result = await Mediator.Send(request);
		if (result.HasError)
		{
			await MainLayout.NotificationService.NotifyError(result);
			return;
		}

		NavigationManager.NavigateTo($"/api/client/dlexport/{fileName}", true);
	}

	async Task ImportFromJson(InputFileChangeEventArgs args)
	{
		var extension = System.IO.Path.GetExtension(args.File.Name);
		if (extension != ".json")
		{
			uploadError = "Ce fichier n'est pas au bon format";
			return;
		}
		var ms = new MemoryStream();
		await args.File.OpenReadStream().CopyToAsync(ms);
		var result = await Mediator.Send(new ImportAccountingPlanFromFileStreamRequest(ms));
		if (result.HasError)
		{
			uploadError = "Une erreur est survenue pendant l'import";
			return;
		}

		uploadEnabled = false;
		uploadSuccess = "Import terminé";

		await LoadDatas();
		await grid!.ReloadAsync();
	}

	async Task Import()
	{
		await Task.Yield();
		uploadEnabled = !uploadEnabled;
		StateHasChanged();
	}

	async Task DeployAll()
	{
		await grid!.ExpandAllAsync();
	}
}
