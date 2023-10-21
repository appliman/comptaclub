using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Datas;
using ComptaClub.Enums;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Requests.Documents;

using MediatR;

using Microsoft.AspNetCore.Components.Web;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class AddDocumentDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;
	[Inject]
	NotificationService NotificationService { get; set; } = default!;
	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Parameter]
	public DocumentData Document { get; set; } = default!;

	CustomValidator customValidator = default!;
	ElementReference fileDropContainer;
	string HoverClass = null!;
	MemoryStream? documentContent = null;

	async Task OnInputFileChange(InputFileChangeEventArgs e)
	{
		IBrowserFile file = e.File;
		if (file == null)
		{
			NotificationService.Notify(NotificationSeverity.Error, "Aucun fichier sélectionné");
			return;
		}

		Document.FileName = file.Name;
		Document.Description = file.Name;
		Document.Size= file.Size;
		Document.MimeType = file.ContentType;
		documentContent = new MemoryStream();
		await file.OpenReadStream().CopyToAsync(documentContent);
		StateHasChanged();
	}

	void OnDragEnter(DragEventArgs e) => HoverClass = "hover";
	void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

	void Select()
	{
		DialogService.Close(documentContent);
	}
}