using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Blazor.Services;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Datas;
using ComptaClub.Extensions;

using Microsoft.AspNetCore.WebUtilities;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class EntryList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;


	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Inject]
	ListFilterQueryStringParametersService ListFilterQueryStringParametersService { get; set; } = default!;

	List<ViewModels.EntryRow>? entryList;
	SuperDataGrid<ViewModels.EntryRow>? grid;
	List<ViewModels.Account> leafAccountList = new();
	ViewModels.Exercice activeExercice = new();
	long currentBalance = 0;
	EntryListFilter filter = new();
	bool filterFirstInitialize = false;
	PeriodFilter? selectedPeriodFilter;
	ViewModels.EntryRow? selectedEntry = default!;
	EntityContext entityContext = default!;

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			ListFilterQueryStringParametersService.AddFilterToQueryString(new FilterInfo(filter));
		}
	}

	protected override async Task OnInitializedAsync()
	{
		var taskList = new List<Task>();

		var t1 = Mediator.Send(new GetActiveExerciceRequest());
		taskList.Add(t1);
		var t2 = Mediator.Send(new GetPlanRequest());
		taskList.Add(t2);

		await Task.WhenAll(taskList);

		var exercice = t1.Result;
		if (exercice == null)
		{
			return;
		}
		activeExercice = Mapping.Profile.ToViewModel(exercice);
		currentBalance = activeExercice.BalanceAmount;

		var accountList = t2.Result;
		leafAccountList = Mapping.Profile.ToViewModels(accountList.GetLeafList().ToList());

		InitializeFilter();

		StateHasChanged();
	}

	void InitializeFilter(bool bypass = false)
	{
		var filterInfo = ListFilterQueryStringParametersService.GetFilterInfoFromQueryString()
					?? new FilterInfo(filter);
		if (bypass)
		{
			filter = new();
			filterInfo = new FilterInfo(filter);
		}

		filter = (filterInfo.Filter as EntryListFilter) ?? new();
		filter.ExerciceId = activeExercice.Id;
		filter.PageSize = int.MaxValue;
		filterFirstInitialize = true;
	}

	async ValueTask<GridItemsProviderResult<ViewModels.EntryRow>> LoadItems(GridItemsProviderRequest<ViewModels.EntryRow> request)
	{
		if (!filterFirstInitialize)
		{
			filter.Search = null;
			if (request.Filters.Any())
			{
				var searchFilter = request.Filters.FirstOrDefault(i => i.PropertyName == nameof(EntryData.PartNumber));
				if (searchFilter is not null
					&& searchFilter.PropertyName == nameof(EntryData.PartNumber))
				{
					filter.Search = searchFilter.PropertyValue;
				}
			}
		}
		if (selectedPeriodFilter is not null)
		{
			filter.FromDayId = selectedPeriodFilter.FromDayId;
			filter.ToDayId = selectedPeriodFilter.ToDayId;
		}
		else
		{
			filter.FromDayId = null;
			filter.ToDayId = null;
		}

		var dataRequest = new GetPagedEntityListRequest<EntryListFilter, Datas.EntryData>(filter);

		var dataPage = await Mediator.Send(dataRequest);
		var list = new List<EntryRow>();
		int rowId = 1;
		foreach (var item in dataPage.List)
		{
			list.Add(new EntryRow
			{
				Entity = item,
				RowIndex = rowId++,
			});
		}
		entryList = list;
		filterFirstInitialize = false;

		if (list.Any())
		{
			entityContext?.ContextChanged(this, list[0], ContextLocation.Bottom);
		}
		return GridItemsProviderResult<ViewModels.EntryRow>.From(
			list.Skip(request.StartIndex).Take(request.Count ?? list.Count).ToList(), list.Count);
	}

	async Task ApplyFilter()
	{
		filter.PageIndex = 0;
		await grid!.ReloadAsync();
	}

	Task InsertRow(string direction)
	{
		NavigationManager.NavigateTo($"/ecriture/ajout/{direction}");
		return Task.CompletedTask;
	}

	string GetEditUrl(ViewModels.EntryRow entry)
	{
		var editUrl = $"/ecriture/edition/{entry.Id}";
		if (QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query)
			.TryGetValue("filter", out var filterValue)
			&& !string.IsNullOrWhiteSpace(filterValue))
		{
			return QueryHelpers.AddQueryString(editUrl, "filter", filterValue.ToString());
		}

		return editUrl;
	}

	async Task DeleteRow(ViewModels.EntryRow entry)
	{
		var confirm = await DialogService.Confirm("Suppression", "Confirmez-vous la suppression de cette écriture ?");
		if (!confirm)
		{
			return;
		}

		var deleteResult = await Mediator.Send(new DeleteEntryRequest(entry.Id));
		if (deleteResult.HasError)
		{
			await NotificationService.NotifyError(deleteResult);
			return;
		}

		await NotificationService.Notify(NotificationSeverity.Success, "L'écriture est maintenant supprimée");
		await grid!.ReloadAsync();
	}

	long? GetProfit()
	{
		if (entryList is null)
		{
			return 0;
		}
		var debit = entryList!.Where(i => i.Entity.AccountDirection == Enums.AccountDirection.Debit).Sum(i => i.Entity.Amount);
		var credit = entryList!.Where(i => i.Entity.AccountDirection == Enums.AccountDirection.Credit).Sum(i => i.Entity.Amount);

		return credit - debit;
	}

	async Task RowSelected(EntryRow entry)
	{
		await Task.Yield();
		selectedEntry = entry;
		entityContext?.ContextChanged(this, entry, ContextLocation.Bottom);
	}
}
