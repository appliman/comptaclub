using AutoMapper;

using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Results;

using MediatR;

namespace ComptaClub.Blazor.Pages;

public partial class DocumentList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	IMapper Mapper { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	List<ViewModels.Document>? documentList;
	RadzenDataGrid<ViewModels.Document> grid = default!;
	DocumentListFilter filter = new();
	ViewModels.Document? documentToUpdate;
	List<BrokenRule> brokenRules = new();

	protected override void OnInitialized()
	{
		filter.PageSize = 100;
	}

	async Task LoadDatas(LoadDataArgs args)
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(filter));
		var vmList = Mapper.Map<List<ViewModels.Document>>(page.List);

		int rowIndex = 1;
		foreach (var item in vmList)
		{
			item.RowIndex = rowIndex++;
		}
		documentList = vmList;
	}

	async Task EditRow(ViewModels.Document doc)
	{
		documentToUpdate = doc;
		await grid!.EditRow(doc);
	}

	async Task SaveRow(ViewModels.Document doc)
	{
		documentToUpdate = null;

		var data = Mapper!.Map<Datas.DocumentData>(doc);
		var saveResult = await Mediator.Send(new SaveDocumentRequest(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid!.UpdateRow(doc);
	}

	void CancelEdit(ViewModels.Document doc)
	{
		documentToUpdate = null;
		grid!.CancelEditRow(doc);
	}

	async Task DeleteRow(ViewModels.Document doc)
	{
		var dialogResult = await DialogService.Confirm("Confirmez-vous la suppression de ce document ?", "Suppression");
		if (dialogResult.Value == false)
		{
			return;
		}

		await Mediator.Send(new DeleteDocumentRequest(doc.Id));
		await grid!.Reload();
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
			await grid!.Reload();
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
			NotificationService.NotifyError(saveResult);
			return;
		}

		await grid.Reload();
		StateHasChanged();
	}
}