using ComptaClub.Datas;
using ComptaClub.Requests.IncomeStatements;
using ComptaClub.Requests;
using MediatR;
using ComptaClub.Blazor.Pages.Shared;
using System.Security.AccessControl;

namespace ComptaClub.Blazor.Pages;

public partial class ViewIncomeStatement
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Parameter]
	public Guid IncomeStatementId { get; set; }

	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	Services.PrintService PrintService { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	ViewModels.IncomeStatement incomeStatement = new();

	protected override async Task OnInitializedAsync()
	{
		var incomeStatementfilter = new IncomeStatementListFilter();
		incomeStatementfilter.IdList.Add(IncomeStatementId);

		var incomeStatementList = await Mediator.Send(new GetPagedEntityListRequest<IncomeStatementListFilter, IncomeStatementData>(incomeStatementfilter));
		incomeStatement = Mapper.Map<ViewModels.IncomeStatement>(incomeStatementList.List.FirstOrDefault());
		if (incomeStatement == null)
		{
			return;
		}

		var incomeStatementItemList = await Mediator.Send(new GetPagedEntityListRequest<IncomeStatementItemListFilter, IncomeStatementItemData>(f => f.IncomeStatementId = incomeStatement.Id));
		incomeStatement.ItemList = Mapper.Map<List<ViewModels.IncomeStatementItem>>(incomeStatementItemList.List);

		foreach (var item in incomeStatement.ItemList)
		{
			item.Level = item.ParentIncomeStatementItemId is null ? 0 : -1;
		}

		// Levelize 
		incomeStatement.ItemList.Levelize();

		// Hierarchize
		incomeStatement.ItemList.Hierarchize();

		MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
		{
			IconName = "print",
			Text = "Imprimer ce compte de résultat",
			OnClick = Print
		})
		.Display();

		StateHasChanged();
	}

	async Task Print()
	{
		await PrintService.Print("#printable");
	}
}