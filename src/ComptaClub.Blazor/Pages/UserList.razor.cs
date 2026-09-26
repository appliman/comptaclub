using System.Threading.Tasks;


using ComptaClub.Contracts.Models.Users;
using ComptaClub.Contracts.Results;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class UserList
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;


	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	List<ViewModels.User>? userList;
	SuperDataGrid<ViewModels.User>? grid;
	readonly UserListFilter filter = new();
	List<BrokenRule> brokenRules = new();
	ViewModels.User? userToInsert;
	ViewModels.User? userToUpdate;

	protected override void OnInitialized()
	{
		filter.PageSize = 100;
	}

	async Task LoadDatas()
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<UserListFilter, Datas.UserData>(filter));
		userList = Mapping.Profile.ToViewModels(page.List);

		int rowIndex = 1;
		foreach (var item in userList)
		{
			item.RowIndex = rowIndex++;
		}
	}

	async ValueTask<GridItemsProviderResult<ViewModels.User>> LoadItems(GridItemsProviderRequest<ViewModels.User> request)
	{
		if (userList is null)
			await LoadDatas();
		IEnumerable<ViewModels.User> rows = userList ?? [];
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"Name" => descending ? rows.OrderByDescending(x => x.Name) : rows.OrderBy(x => x.Name),
			"Email" => descending ? rows.OrderByDescending(x => x.Email) : rows.OrderBy(x => x.Email),
			"CreationDate" => descending ? rows.OrderByDescending(x => x.CreationDate) : rows.OrderBy(x => x.CreationDate),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		var items = rows.ToList();
		return GridItemsProviderResult<ViewModels.User>.From(items.Skip(request.StartIndex).Take(request.Count ?? items.Count).ToList(), items.Count);
	}

	async Task ReloadItems()
	{
		userList = null;
		if (grid is not null) await grid.ReloadAsync();
	}

	async Task InsertRow()
	{
		var data = await Mediator.Send(new CreateUserRequest());
		userToInsert = Mapping.Profile.ToViewModel(data);
		userList ??= [];
		userList.Insert(0, userToInsert);
		await grid!.ReloadAsync();
		await grid.BeginEditAsync(userToInsert);
	}

	async Task EditRow(ViewModels.User user)
	{
		userToUpdate = user;
		await grid!.BeginEditAsync(user);
	}

	async Task SaveRow(ViewModels.User user)
	{
		var data = Mapping.Profile.ToData(user);
		var saveResult = await Mediator.Send(new SaveEntityRequest<Datas.UserData>(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		userToInsert = null;
		userToUpdate = null;
		await grid!.EndEditAsync(user);
		await ReloadItems();
	}

	async Task CancelEdit(ViewModels.User user)
	{
		if (user == userToInsert)
		{
			userToInsert = null;
		}

		userToUpdate = null;

		await grid!.CancelEditAsync(user);
		await ReloadItems();
	}

	async Task DeleteRow(ViewModels.User user)
	{
		var result = await Mediator.Send(new DisableUserRequest(user.Id));
		if (result.HasError)
		{
			await NotificationService.NotifyError(result);
		}
		await ReloadItems();
	}
}
