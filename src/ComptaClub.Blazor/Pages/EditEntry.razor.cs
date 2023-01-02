using ComptaClub.Blazor.Pages.Components;

namespace ComptaClub.Blazor.Pages;

public partial class EditEntry : ComponentBase
{
	[Parameter]
	public Guid? EntryId { get; set; }

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;


	ViewModels.Entry entry = new();
	CustomValidator? customValidator;

	protected override async Task OnInitializedAsync()
	{
		if (EntryId == null
			|| EntryId == Guid.Empty)
		{
			var data = await Mediator.Send(new Requests.CreateEntryRequest());
			entry = Mapper.Map<ViewModels.Entry>(data);
		}
		else
		{
			var data = await Mediator.Send(new Requests.GetEntryByFilterRequest(f => f.Id == EntryId.Value));
			if (data != null)
			{
				entry = Mapper.Map<ViewModels.Entry>(data);
			}
		}
	}

	async Task ValidateAndSave()
	{
		var data = Mapper.Map<Datas.EntryData>(entry);
		var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.EntryData>(data));
		if (saveResult!.HasError)
		{
			customValidator!.DisplayErrors(saveResult.ErrorBrokenRuleList);
			return;
		}

		NavigationManager.NavigateTo("/ecritures");
	}

}