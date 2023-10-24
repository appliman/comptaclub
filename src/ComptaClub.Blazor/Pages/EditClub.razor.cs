using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Requests.Clubs;

namespace ComptaClub.Blazor.Pages;

public partial class EditClub
{
	[CascadingParameter]
	Shared.MainLayout MainLayout { get; set; } = default!;

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

	Datas.ClubData club = new();
	CustomValidator customValidator = default!;

	protected override async Task OnInitializedAsync()
	{
		club = await Mediator.Send(new GetClubRequest());

		MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
		{
			IconName = "save",
			Text = "Sauvegarder",
			Title = "Sauvegarder les informations",
			OnClick = ValidateAndSave
		}).Display();
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

}