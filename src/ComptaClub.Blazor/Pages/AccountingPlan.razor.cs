using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Blazor.Pages;

public partial class AccountingPlan : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;


	IEnumerable<ViewModels.Account> accountList = new List<ViewModels.Account>();
	RadzenDataGrid<ViewModels.Account>? grid;
	ViewModels.Account? accountToUpdate;
	ViewModels.Account? accountToInsert;
	List<BrokenRule> brokenRules = new();
	bool uploadEnabled = false;
	string? uploadError = null;
	string? uploadSuccess = null;

	protected override async Task OnInitializedAsync()
	{
		await LoadDatas();
	}

	async Task LoadDatas()
	{
		var dataPlan = await Mediator.Send(new GetPlanRequest());
		var list = MapPlan(dataPlan);
		accountList = list;
	}

	List<ViewModels.Account> MapPlan(List<Datas.AccountData> list)
	{
		var result = new List<ViewModels.Account>();
		foreach (var item in list)
		{
			var account = Mapper.Map<ViewModels.Account>(item);
			account.Children = MapPlan(item.Children);
			result.Add(account);
		}
		return result;
	}

	void RowRender(RowRenderEventArgs<ViewModels.Account> args)
	{
		args.Expandable = args.Data.Children.Any();
	}

	void LoadChildData(DataGridLoadChildDataEventArgs<ViewModels.Account> args)
	{
		args.Data = args.Item.Children;
	}

	async Task EditRow(ViewModels.Account account)
	{
		accountToUpdate = account;
		await grid!.EditRow(account);
	}

	async Task SaveRow(ViewModels.Account account)
	{
		if (account == accountToInsert)
		{
			accountToInsert = null;
		}

		accountToUpdate = null;

		var data = Mapper.Map<Datas.AccountData>(account);
		var saveResult = await Mediator.Send(new SaveEntityRequest<Datas.AccountData>(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid!.UpdateRow(account);
		brokenRules.Clear();
	}

	void CancelEdit(ViewModels.Account account)
	{
		if (account == accountToInsert)
		{
			accountToInsert = null;
		}

		accountToUpdate = null;

		grid!.CancelEditRow(account);
	}

	async Task InsertRow()
	{
		var data = await Mediator.Send(new CreateAccountRequest());
		accountToInsert = Mapper.Map<ViewModels.Account>(data);
		await grid!.InsertRow(accountToInsert);
	}

	async Task InsertRow(ViewModels.Account account)
	{
		await grid!.ExpandRow(account);
		var data = await Mediator.Send(new CreateAccountRequest()
		{
			Direction = account.Direction,
			ParentId = account.Id
		});
		accountToInsert = Mapper.Map<ViewModels.Account>(data);
		account.Children.Add(accountToInsert);
		await grid.SelectRow(accountToInsert);
		await grid.EditRow(accountToInsert);
	}

	async Task DeleteRow(ViewModels.Account account)
	{
		var confirm = await MainLayout.DialogService.Confirm("Confirmez vous la suppression de ce compte", "Suppression");
		if (!confirm.GetValueOrDefault(false))
		{
			return;
		}
		var result = await Mediator!.Send(new DeleteAccountRequest(account.Id));
		if (result.HasError)
		{
			MainLayout.NotificationService.NotifyError(result);
		}
		else
		{
			await LoadDatas();
			await grid!.Reload();
			MainLayout.NotificationService.Notify(new NotificationMessage
			{
				Severity = NotificationSeverity.Info,
				Summary = $"Ce compte ({account.Code}) vient d'etre supprimé"
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
			MainLayout.NotificationService.NotifyError(result);
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
		await grid!.Reload();
	}

	async Task Import()
	{
		await Task.Yield();
		uploadEnabled = !uploadEnabled;
		StateHasChanged();
	}

	async Task DeployAll()
	{
		foreach (var row in accountList)
		{
			await grid!.ExpandRow(row);
		}
	}
}