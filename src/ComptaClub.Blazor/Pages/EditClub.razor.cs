using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Contracts.Models.Clubs;

namespace ComptaClub.Blazor.Pages;

public partial class EditClub
{
    [Inject]
    ComptaClub.Configuration.ComptaClubSettings Settings { get; set; } = default!;

    [Inject]
    NavigationManager NavigationManager { get; set; } = default!;

    private bool _creatingBackup;
    private string? _backupUrl;

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

    private async Task CreateDatabaseBackup()
    {
        if (_creatingBackup)
        {
            return;
        }
        _creatingBackup = true;
        _backupUrl = null;
        try
        {
            var _result = await Mediator.Send(new CreateZippedDatabaseRequest(MainLayout.GetCurrentUser().Id, NavigationManager.BaseUri));
            if (_result.HasError)
            {
                await NotificationService.Notify(NotificationSeverity.Error, _result.GetAllBrokenRules());
                return;
            }
            _backupUrl = _result.RelativeUrl;
            await NotificationService.Notify(NotificationSeverity.Success, "Sauvegarde prête. Le mot de passe vous a été envoyé par email.");
        }
        finally
        {
            _creatingBackup = false;
        }
    }
}
