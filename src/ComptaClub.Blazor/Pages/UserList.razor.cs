using AutoMapper;

using ComptaClub.Blazor.ViewModels;
using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Blazor.Pages;

public partial class UserList
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	IMapper Mapper { get; set; } = default!;

	List<ViewModels.User>? userList;
	RadzenDataGrid<ViewModels.User>? grid = new();
	readonly UserListFilter filter = new();
	List<Results.BrokenRule> brokenRules = new();
	ViewModels.User? userToInsert;
	ViewModels.User? userToUpdate;

	protected override void OnInitialized()
	{
		MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
		{
			IconName = "person_add",
			Text = "Ajouter",
			Title = "Ajouter un utilisateur",
			OnClick = InsertRow
		}).Display();

		filter.PageSize = 100;
	}

	async Task LoadDatas(LoadDataArgs args)
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<UserListFilter, Datas.UserData>(filter));
		userList = Mapper.Map<List<ViewModels.User>>(page.List);

		int rowIndex = 1;
		foreach (var item in userList)
		{
			item.RowIndex = rowIndex++;
		}
	}

	async Task InsertRow()
	{
		var data = await Mediator.Send(new Requests.Users.CreateUserRequest());
		userToInsert = Mapper.Map<ViewModels.User>(data);
		await grid!.InsertRow(userToInsert);
	}

	async Task EditRow(ViewModels.User user)
	{
		userToUpdate = user;
		await grid!.EditRow(user);
	}

	async Task SaveRow(ViewModels.User user)
	{
		if (user == userToInsert)
		{
			userToInsert = null;
		}

		userToUpdate = null;

		var data = Mapper!.Map<Datas.UserData>(user);
		var saveResult = await Mediator.Send(new Requests.SaveEntityRequest<Datas.UserData>(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid!.UpdateRow(user);
	}

	void CancelEdit(ViewModels.User	user)
	{
		if (user == userToInsert)
		{
			userToInsert = null;
		}

		userToUpdate = null;

		grid!.CancelEditRow(user);
	}

	void DeleteRow(ViewModels.User user)
	{
		// Todo
	}
}