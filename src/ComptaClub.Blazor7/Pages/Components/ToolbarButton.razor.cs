namespace ComptaClub.Blazor.Pages.Components;

public partial class ToolbarButton
{
	[Inject]
	DialogService DialogService { get; set; } = default!;

    [Parameter]
	public ViewModels.Toolbar.ToolbarButton ToolbarItem { get; set; } = default!;

	protected string? disabled;
	protected bool loading = false;

	public async Task OnClick()
	{
		if (loading)
		{
			return;
		}

		if (ToolbarItem.NeedConfirmation)
		{
            var message = ToolbarItem.ConfirmationMessage;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = $"Confirmez vous cette action : {ToolbarItem.Description} ?";
            }

            var dialogResult = await DialogService.Confirm(message, "Veuillez confirmer");
			if (!dialogResult.Value)
			{
				return;
			}
		}

		loading = true;
		disabled = "disabled";
		StateHasChanged();

        ToolbarItem.OnClick?.Invoke();

        loading = false;
		disabled = null;
	}
}