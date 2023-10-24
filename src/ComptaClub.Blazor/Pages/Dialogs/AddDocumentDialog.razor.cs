using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Datas;
using ComptaClub.Enums;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Requests.Documents;

using MediatR;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

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
	IJSRuntime JSRuntime { get; set; }


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

	async Task Select()
	{
        if (frameUri is not null)
        {
            await JSRuntime.InvokeVoidAsync("camera.stopCamera", "videoFeed");
        }
        DialogService.Close(documentContent);
    }

	async Task CloseDialog()
    {
        if (frameUri is not null)
        {
            await JSRuntime.InvokeVoidAsync("camera.stopCamera", "videoFeed");
        }
        DialogService.Close();
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

	[JSInvokable]
	public async Task ProcessImage(string imageString)
	{
		if (string.IsNullOrWhiteSpace(imageString))
		{
			return;
		}
		imageDataBase64 = imageString.Split(',')[1];
		frameUri = imageString;

		using var image = Image.FromStream(new MemoryStream(Convert.FromBase64String(imageDataBase64)));
		documentContent = new MemoryStream();
		image.Save(documentContent, ImageFormat.Jpeg);
		Document.FileName = $"image{DateTime.Now:dd-MM-yy}.jpg";
		Document.Description = "Photo";
		Document.Size = documentContent.Length;
		Document.MimeType = "image/jpg";
		await JSRuntime.InvokeVoidAsync("camera.stopCamera", "videoFeed");
		StateHasChanged();
	}
}