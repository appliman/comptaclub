using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Datas;

using MediatR;

using Microsoft.AspNetCore.Components.Web;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class AddOrAttachDocumentToEntityDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;
	[Inject]
	NotificationService NotificationService { get; set; } = default!;
	[Inject]
	DialogService DialogService { get; set; } = default!;
	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

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
	RadzenDataGrid<ViewModels.Document> grid = default!;
	string panel = "grid";
	DocumentData? document;

	protected override void OnInitialized()
	{
		documentFilter.PageSize = int.MaxValue;
	}

	async Task LoadDatas(LoadDataArgs args)
	{
		// Recupération de la liste des documents
		var page = await Mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(documentFilter));
		documentList = Mapper.Map<IEnumerable<ViewModels.Document>>(page.List);
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
				NotificationService.NotifyError(saveDocumentResult);
				return;
			}

			selectedDocuments.Add(Mapper.Map<ViewModels.Document>(document));
		}

		DialogService.Close(selectedDocuments);
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
			NotificationService.Notify(NotificationSeverity.Error, "Aucun fichier sélectionné");
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