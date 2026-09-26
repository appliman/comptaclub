
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Members;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages.Shared;

public partial class AssociatedMemberByEntry
{
	[Parameter]
	public Guid? EntryId { get; set; }

	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;


	[Inject]
	DialogService DialogService { get; set; } = default!;

	protected SuperDataGrid<AssociatedMemberToEntryRow> grid = default!;
    protected List<AssociatedMemberToEntryRow>? associatedMemberList;
    protected AssociatedMemberToEntryRow? associationToInsert;
    protected AssociatedMemberToEntryRow? associationToUpdate;
    protected List<AssociatedMemberToEntryRow> unlinkedAssociationList = new();
    protected long total = 0;

	public async Task LoadDatas()
	{
		associatedMemberList = new();
		total = 0;

		if (EntryId == null)
		{
			return;
		}
		var dataList = await Mediator.Send(new GetAssociatedMemberListByEntryRequest(EntryId.Value));
		var memberIdList = dataList.Select(i => i.MemberId).Distinct().ToList();
		var memberPage = await Mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(f =>
		{
			f.IdList = memberIdList;
		}));

		List<AssociatedMemberToEntryRow> list = new();
		foreach (var item in dataList)
		{
			if (unlinkedAssociationList.Any(i => i.Member.Id == item.MemberId))
			{
				continue;
			}
			var member = memberPage.List.SingleOrDefault(i => i.Id == item.MemberId);
			if (member is null)
			{
				continue;
			}
			var association = new AssociatedMemberToEntryRow
			{
				Amount = item.Amount,
				Member = new MemberRow
				{
					Entity = member
				}
			};
			list.Add(association);
			total = total + association.Amount;
		}
		associatedMemberList = list;
	}

	protected async ValueTask<GridItemsProviderResult<AssociatedMemberToEntryRow>> LoadItems(GridItemsProviderRequest<AssociatedMemberToEntryRow> request)
	{
		if (associatedMemberList is null)
			await LoadDatas();
		var rows = associatedMemberList ?? [];
		return GridItemsProviderResult<AssociatedMemberToEntryRow>.From(
			rows.Skip(request.StartIndex).Take(request.Count ?? rows.Count).ToList(), rows.Count);
	}

	protected async Task ReloadItems()
	{
		associatedMemberList = null;
		await grid.ReloadAsync();
	}

	public async Task SaveAssociations()
	{
		foreach (var association in associatedMemberList!)
		{
			await Mediator.Send(new LinkMemberToEntryRequest(EntryId!.Value, association.Member.Id, association.Amount));
		}
		foreach (var association in unlinkedAssociationList)
		{
			await Mediator.Send(new UnlinkMemberToEntryRequest(association.Id));
		}
	}

	public async Task Save(AssociatedMemberToEntryRow row)
	{
		if (row == associationToInsert)
		{
			associationToInsert = null;
		}

		associationToUpdate = null;
		await SaveAssociations();
		await grid.EndEditAsync(row);
		await ReloadItems();
	}

	public async Task InsertRow()
	{
		var result = await DialogService.OpenAsync<Dialogs.MemberSelectorDialog>("Selection d'un membre",
			options: new DialogOptions
			{

			});

		var member = result as MemberRow;
		if (member != null)
		{
			var entry = await Mediator.Send(new GetEntryByFilterRequest(f => f.GetById(EntryId!.Value)));
			var entryAmount = entry!.Amount;

			associationToInsert = new();
			associationToInsert.Member = member;
			associationToInsert.Amount = Math.Max(0, entryAmount - total);
			associatedMemberList ??= [];
			associatedMemberList!.Add(associationToInsert);
			await grid.ReloadAsync();
			await grid.BeginEditAsync(associationToInsert);
		}
	}

	protected async Task EditRow(AssociatedMemberToEntryRow row)
	{
		associationToUpdate = row;
		await grid.BeginEditAsync(row);
	}

    protected async Task CancelEdit(AssociatedMemberToEntryRow row)
	{
		if (row == associationToInsert)
		{
			associationToInsert = null;
		}

		associationToUpdate = null;

		await grid.CancelEditAsync(row);
		await ReloadItems();
	}


    protected async Task DeleteRow(AssociatedMemberToEntryRow row)
	{
		if (row == associationToInsert)
		{
			associationToInsert = null;
		}
		if (row == associationToUpdate)
		{
			associationToUpdate = null;
		}
		if (!unlinkedAssociationList.Any(i => i.Id == row.Id))
		{
			unlinkedAssociationList.Add(row);
			await ReloadItems();
		}
	}
}
