using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Contracts.Models.Clubs;

namespace ComptaClub.Blazor.Pages;

public partial class EditClub
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	Datas.ClubData club = new();
	CustomValidator customValidator = default!;

	protected override async Task OnInitializedAsync()
	{
		club = await Mediator.Send(new GetClubRequest());
	}

	async Task ValidateAndSave()
	{
		var saveResult = await Mediator!.Send(new SaveClubRequest(club));
		if (saveResult!.HasError)
		{
			customValidator.DisplayErrors(saveResult.ErrorBrokenRuleList);
			return;
		}
	}

	async Task OnInputFileChange(InputFileChangeEventArgs args)
	{
		IBrowserFile file = args.File;
		if (file == null)
		{
			await NotificationService.Notify(NotificationSeverity.Error, "Aucun fichier sélectionné");
			return;
		}

		// Verifier s'il s'agit bien d'une image
		if (!file.ContentType.StartsWith("image/"))
		{
			await NotificationService.Notify(NotificationSeverity.Error, "Le fichier sélectionné n'est pas une image");
			return;
		}

		using var ms = new MemoryStream();
		await file.OpenReadStream().CopyToAsync(ms);
		club.LogoBase64String = Convert.ToBase64String(ms.ToArray());
		club.LogoContentType = file.ContentType;
		StateHasChanged();
	}
}
