using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Datas;

using MediatR;

using Microsoft.AspNetCore.Components.Web;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class ImportMemberExcelFileDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;
	[Inject]
	NotificationService NotificationService { get; set; } = default!;
	[Inject]
	DialogService DialogService { get; set; } = default!;

	CustomValidator customValidator = default!;
	ElementReference fileDropContainer;
	string HoverClass = null!;
	MemoryStream documentContent = new();
	string? fileName;

	async Task OnInputFileChange(InputFileChangeEventArgs e)
	{
		IBrowserFile file = e.File;
		if (file == null)
		{
			customValidator.DisplayError("Fichier invalide");
			NotificationService.Notify(NotificationSeverity.Error, "Aucun fichier sélectionné");
			return;
		}
	
		var extension = Path.GetExtension(file.Name);
		if (extension != ".xlsx")
		{
			customValidator.DisplayError("Le fichier doit être au format Excel (.xlsx)");
			NotificationService.Notify(NotificationSeverity.Error, "Le fichier doit être au format Excel (.xlsx)");
			return;
		}

		fileName = file.Name;

		documentContent = new MemoryStream();
		await file.OpenReadStream().CopyToAsync(documentContent);
		StateHasChanged();
	}

	void OnDragEnter(DragEventArgs e)
	{
		HoverClass = "hover";
	}

	void OnDragLeave(DragEventArgs e)
	{
		HoverClass = string.Empty;
	}

	void Select()
	{
		DialogService.Close(documentContent);
	}
}