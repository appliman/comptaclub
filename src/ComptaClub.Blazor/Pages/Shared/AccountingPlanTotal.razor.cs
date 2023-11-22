using ComptaClub.Blazor.Services;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;

using MediatR;

namespace ComptaClub.Blazor.Pages.Shared
{
	public partial class AccountingPlanTotal
	{
		[Parameter]
		public IEnumerable<ViewModels.Account> Plan { get; set; } = new List<ViewModels.Account>();

		[Parameter]
		public Enums.AccountDirection Direction { get; set; } = Enums.AccountDirection.Credit;

		[Parameter]
		public string Title { get; set; } = null!;

		[Inject]
		ListFilterQueryStringParametersService ListFilterQueryStringParametersService { get; set; } = default!;

		[Inject]
		IMediator Mediator { get; set; } = default!;

		async Task DisplayEntries(Guid accountId)
		{
			var activeExercice = await Mediator.Send(new GetActiveExerciceRequest());
			if (activeExercice is null)
			{
				return;
			}
			var filter = new EntryListFilter();
			filter.ExerciceId = activeExercice.Id;
			filter.PageSize = int.MaxValue;
			filter.AccountIdList = new List<Guid>() { accountId };
			filter.UseDeepAccount = true;

			ListFilterQueryStringParametersService.AddFilterToQueryString(new FilterInfo(filter), "/ecritures");
		}
	}
}