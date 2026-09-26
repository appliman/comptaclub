using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Datas;

using ChannelMediator;

using Microsoft.AspNetCore.Components.Web;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class AddOrAttachDocumentToEntityDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;
	[Inject]
	NotificationService NotificationService { get; set; } = default!;
	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Parameter]
	public MetaEntityId MetaEntityId { get; set; } = default!;

	IBrowserFile SelectedFile { get; set; } = default!;
	CustomValidator customValidator = default!;
	ElementReference fileDropContainer;
	string HoverClass = null!;
	MemoryStream? documentContent = null;
	IEnumerable<ViewModels.Document>? documentList;
	DocumentListFilter documentFilter = new();
	IList<ViewModels.Document>? selectedDocuments;
	SuperDataGrid<ViewModels.Document> grid = default!;
	string panel = "grid";
	DocumentData? document;

	protected override void OnInitialized()
	{
		documentFilter.PageSize = int.MaxValue;
	}

	async Task LoadDatas()
	{
		// Recup�ration de la liste des documents
		var page = await Mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(documentFilter));
		documentList = Mapping.Profile.ToViewModels(page.List);
	}

	void OnSelectionChanged(IEnumerable<ViewModels.Document> selected) => selectedDocuments = selected.ToList();

	async ValueTask<GridItemsProviderResult<ViewModels.Document>> LoadItems(GridItemsProviderRequest<ViewModels.Document> request)
	{
		if (documentList is null)
        {
            await LoadDatas();
        }

        IEnumerable<ViewModels.Document> rows = documentList ?? [];
		foreach (var filter in request.Filters)
		{
			if (string.IsNullOrWhiteSpace(filter.PropertyValue))
            {
                continue;
            }

            rows = filter.PropertyName switch
			{
				"FileName" => rows.Where(x => x.FileName.Contains(filter.PropertyValue, StringComparison.OrdinalIgnoreCase)),
				"Description" => rows.Where(x => x.Description.Contains(filter.PropertyValue, StringComparison.OrdinalIgnoreCase)),
				_ => rows
			};
		}
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"FileName" => descending ? rows.OrderByDescending(x => x.FileName) : rows.OrderBy(x => x.FileName),
			"Description" => descending ? rows.OrderByDescending(x => x.Description) : rows.OrderBy(x => x.Description),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		var items = rows.ToList();
		return GridItemsProviderResult<ViewModels.Document>.From(items.Skip(request.StartIndex).Take(request.Count ?? items.Count).ToList(), items.Count);
	}

	void OnDragEnter(DragEventArgs e)
	{
		HoverClass = "hover";
	}

	void OnDragLeave(DragEventArgs e)
	{
		HoverClass = string.Empty;
	}

	async Task Select()
	{
		if (selectedDocuments is null)
		{
			selectedDocuments = new List<ViewModels.Document>();
		}

		if (document is not null)
		{
			var saveDocumentResult = await Mediator.Send(new SaveDocumentRequest(document, documentContent));
			if (saveDocumentResult.HasError)
			{
				await NotificationService.NotifyError(saveDocumentResult);
				return;
			}

			selectedDocuments.Add(Mapping.Profile.ToViewModel(document));
		}

		await DialogService.Close(selectedDocuments);
	}

	async Task Upload()
	{
		document = await Mediator.Send(new CreateDocumentRequest());
		panel = "upload";
		StateHasChanged();
	}

	async Task OnInputFileChange(InputFileChangeEventArgs e)
	{
		IBrowserFile file = e.File;
		if (file == null)
		{
			await NotificationService.Notify(NotificationSeverity.Error, "Aucun fichier s�lectionn�");
			return;
		}
		document!.FileName = file.Name;
		document.Description = file.Name;
		document.Size = file.Size;
		document.MimeType = !string.IsNullOrWhiteSpace(file.ContentType) ? file.ContentType : "application/octet-stream";
		documentContent = new MemoryStream();
		await file.OpenReadStream().CopyToAsync(documentContent);
		StateHasChanged();
	}
}
