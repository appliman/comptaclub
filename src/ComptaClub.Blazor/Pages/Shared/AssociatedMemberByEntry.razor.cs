using AutoMapper;

using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Blazor.Pages.Shared;

public partial class AssociatedMemberByEntry
{
	[Parameter]
	public Guid? EntryId { get; set; }

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

	[Inject]
	IMapper Mapper { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;

	protected RadzenDataGrid<AssociatedMemberToEntryRow> grid = default!;
    protected List<AssociatedMemberToEntryRow>? associatedMemberList;
    protected AssociatedMemberToEntryRow? associationToInsert;
    protected AssociatedMemberToEntryRow? associationToUpdate;
    protected List<AssociatedMemberToEntryRow> unlinkedAssociationList = new();
    protected long total = 0;

	public async Task LoadDatas(LoadDataArgs args)
	{
		associatedMemberList = new();

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
		StateHasChanged();
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
		total = total + row.Amount;
		await grid.UpdateRow(row);
	}

	public async Task InsertRow()
	{
		var result = await DialogService.OpenAsync<Dialogs.MemberSelectorDialog>("Selection d'un membre",
			options: new DialogOptions
			{
				CloseDialogOnEsc = true,
			});

		var member = result as MemberRow;
		if (member != null)
		{
			var entry = await Mediator.Send(new GetEntryByFilterRequest(f => f.GetById(EntryId!.Value)));
			var entryAmount = entry!.Amount;

			associationToInsert = new();
			associationToInsert.Member = member;
			associationToInsert.Amount = Math.Max(0, entryAmount - total);
			await grid.InsertRow(associationToInsert);
			associatedMemberList!.Add(associationToInsert);
		}
	}

	protected async Task EditRow(AssociatedMemberToEntryRow row)
	{
		associationToUpdate = row;
		await grid.EditRow(row);
	}

    protected void CancelEdit(AssociatedMemberToEntryRow row)
	{
		if (row == associationToInsert)
		{
			associationToInsert = null;
		}

		associationToUpdate = null;

		grid.CancelEditRow(row);
	}


    protected Task DeleteRow(AssociatedMemberToEntryRow row)
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
			grid.Reload();
		}
		return Task.CompletedTask;
	}
}