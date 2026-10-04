using ComptaClub.Blazor.Services;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ChannelMediator;

namespace ComptaClub.Blazor.Pages.Shared;

public partial class AccountingPlanTotal
{
    [Parameter]
    public IEnumerable<ViewModels.Account> Plan { get; set; } = new List<ViewModels.Account>();

    [Parameter]
    public Guid? PreviousExerciceId { get; set; }

    [Parameter]
    public Enums.AccountDirection Direction { get; set; } = Enums.AccountDirection.Credit;

    [Parameter]
    public string Title { get; set; } = null!;

    [Inject]
    ListFilterQueryStringParametersService ListFilterQueryStringParametersService { get; set; } = default!;

    [Inject]
    IMediator Mediator { get; set; } = default!;

    private async Task DisplayEntries(Guid accountId, Guid? exerciceId = null)
    {
        var _exerciceId = exerciceId;
        if (!_exerciceId.HasValue)
        {
            var _activeExercice = await Mediator.Send(new GetActiveExerciceRequest());
            _exerciceId = _activeExercice?.Id;
        }

        if (!_exerciceId.HasValue)
        {
            return;
        }

        var _filter = new EntryListFilter
        {
            ExerciceId = _exerciceId.Value,
            PageSize = int.MaxValue,
            AccountIdList = new List<Guid> { accountId },
            UseDeepAccount = true
        };

        ListFilterQueryStringParametersService.AddFilterToQueryString(new FilterInfo(_filter), "/ecritures");
    }
}
