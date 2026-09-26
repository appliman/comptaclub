
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Results;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class DocumentList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;


	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	List<ViewModels.Document>? documentList;
	SuperDataGrid<ViewModels.Document> grid = default!;
	DocumentListFilter filter = new();
	ViewModels.Document? documentToUpdate;
	List<BrokenRule> brokenRules = new();

	protected override void OnInitialized()
	{
		filter.PageSize = 100;
	}

	async Task LoadDatas()
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(filter));
		var vmList = Mapping.Profile.ToViewModels(page.List);

		int rowIndex = 1;
		foreach (var item in vmList)
		{
			item.RowIndex = rowIndex++;
		}
		documentList = vmList;
	}

	async ValueTask<GridItemsProviderResult<ViewModels.Document>> LoadItems(GridItemsProviderRequest<ViewModels.Document> request)
	{
		if (documentList is null)
			await LoadDatas();
		IEnumerable<ViewModels.Document> rows = documentList ?? [];
		foreach (var filterInfo in request.Filters)
		{
			if (string.IsNullOrWhiteSpace(filterInfo.PropertyValue)) continue;
			rows = filterInfo.PropertyName switch
			{
				"FileName" => rows.Where(x => x.FileName.Contains(filterInfo.PropertyValue, StringComparison.OrdinalIgnoreCase)),
				"Description" => rows.Where(x => x.Description.Contains(filterInfo.PropertyValue, StringComparison.OrdinalIgnoreCase)),
				_ => rows
			};
		}
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"FileName" => descending ? rows.OrderByDescending(x => x.FileName) : rows.OrderBy(x => x.FileName),
			"Description" => descending ? rows.OrderByDescending(x => x.Description) : rows.OrderBy(x => x.Description),
			"LastUpdate" => descending ? rows.OrderByDescending(x => x.LastUpdate) : rows.OrderBy(x => x.LastUpdate),
			"Size" => descending ? rows.OrderByDescending(x => x.Size) : rows.OrderBy(x => x.Size),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		var items = rows.ToList();
		return GridItemsProviderResult<ViewModels.Document>.From(items.Skip(request.StartIndex).Take(request.Count ?? items.Count).ToList(), items.Count);
	}

	async Task ReloadItems()
	{
		documentList = null;
		await grid.ReloadAsync();
	}

	async Task EditRow(ViewModels.Document doc)
	{
		documentToUpdate = doc;
		await grid.BeginEditAsync(doc);
	}

	async Task SaveRow(ViewModels.Document doc)
	{
		var data = Mapping.Profile.ToData(doc);
		var saveResult = await Mediator.Send(new SaveDocumentRequest(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		documentToUpdate = null;
		await grid.EndEditAsync(doc);
		await ReloadItems();
	}

	async Task CancelEdit(ViewModels.Document doc)
	{
		documentToUpdate = null;
		await grid.CancelEditAsync(doc);
		await ReloadItems();
	}

	async Task DeleteRow(ViewModels.Document doc)
	{
		var dialogResult = await DialogService.Confirm("Suppression", "Confirmez-vous la suppression de ce document ?");
		if (!dialogResult)
		{
			return;
		}

		await Mediator.Send(new DeleteDocumentRequest(doc.Id));
		await ReloadItems();
	}

	async Task AddDocument()
	{
		var document = await Mediator.Send(new CreateDocumentRequest());
		var dialogResult = await DialogService.OpenAsync<Dialogs.AddDocumentDialog>("Ajouter un document", new Dictionary<string, object>
		{
			{ "Document", document }
		});

		if (dialogResult != null)
		{
			await ReloadItems();
		}

		var documentContent = dialogResult as MemoryStream;
		if (documentContent is null)
		{
			return;
		}

		var saveResult = await Mediator.Send(new SaveDocumentRequest(document, documentContent));
		documentContent.Dispose();

		if (saveResult.HasError)
		{
			await NotificationService.NotifyError(saveResult);
			return;
		}

		await ReloadItems();
		StateHasChanged();
	}
}
