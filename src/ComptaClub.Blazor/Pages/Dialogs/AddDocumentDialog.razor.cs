using System.Drawing;
using System.Drawing.Imaging;

using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Datas;

using ChannelMediator;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class AddDocumentDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;
	[Inject]
	NotificationService NotificationService { get; set; } = default!;
	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Inject]
	IJSRuntime JSRuntime { get; set; } = default!;


	[Parameter]
	public DocumentData Document { get; set; } = default!;

	CustomValidator customValidator = default!;
	string HoverClass = null!;
	MemoryStream? documentContent = null;
	string? imageDataBase64;
	string? frameUri;

	async Task OnInputFileChange(InputFileChangeEventArgs e)
	{
		IBrowserFile file = e.File;
		if (file == null)
		{
			await NotificationService.Notify(NotificationSeverity.Error, "Aucun fichier sélectionné");
			return;
		}

		Document.FileName = file.Name;
		Document.Description = file.Name;
		Document.Size = file.Size;
		Document.MimeType = file.ContentType;
		if (string.IsNullOrWhiteSpace(Document.MimeType))
		{
			Document.MimeType = "application/octet-stream";
		}
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

	async Task Select()
	{
		if (frameUri is not null)
		{
			await JSRuntime.InvokeVoidAsync("camera.stopCamera", "videoFeed");
		}
		await DialogService.Close(documentContent);
	}

	async Task CloseDialog()
	{
		if (frameUri is not null)
		{
			await JSRuntime.InvokeVoidAsync("camera.stopCamera", "videoFeed");
		}
		await DialogService.Close();
	}

	void TabChanged(int tabId)
	{
		if (tabId == 1)
		{
			JSRuntime.InvokeVoidAsync("camera.startCamera", "videoFeed");
		}
		else
		{
			JSRuntime.InvokeVoidAsync("camera.stopCamera", "videoFeed");
		}
	}

	void StartCamera()
	{
		frameUri = null;
		JSRuntime.InvokeVoidAsync("camera.startCamera", "videoFeed");
		StateHasChanged();
	}

	async Task CaptureFrame()
	{
		try
		{
			await JSRuntime.InvokeAsync<string>("camera.takePicture", "videoFeed", "currentFrame", DotNetObjectReference.Create(this));
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
}