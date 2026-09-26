using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Datas;

using ChannelMediator;

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
	string HoverClass = null!;
	string? tempFileName;
	string? fileName;
	long fileSize = 0;

	async Task OnInputFileChange(InputFileChangeEventArgs e)
	{
		IBrowserFile file = e.File;
		if (file == null)
		{
			customValidator.DisplayError("Fichier invalide");
			await NotificationService.Notify(NotificationSeverity.Error, "Aucun fichier s�lectionn�");
			return;
		}
	
		var extension = Path.GetExtension(file.Name);
		if (extension != ".xlsx")
		{
			customValidator.DisplayError("Le fichier doit �tre au format Excel (.xlsx)");
			await NotificationService.Notify(NotificationSeverity.Error, "Le fichier doit �tre au format Excel (.xlsx)");
			return;
		}

		fileName = file.Name;
		fileSize = file.Size;

		// Ecrire le stream dans un fichier temporaire
		tempFileName = System.IO.Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.xlsx");
		using var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write);
		await file.OpenReadStream().CopyToAsync(fileStream);
		fileStream.Close();
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

	async Task Select()
	{
		await DialogService.Close(tempFileName);
	}
}