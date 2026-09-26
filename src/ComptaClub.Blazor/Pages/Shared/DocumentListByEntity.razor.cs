
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Results;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages.Shared;

public partial class DocumentListByEntity
{
	[Parameter]
	public MetaEntity MetaEntity { get; set; }

	[Parameter]
	public Guid EntityId { get; set; }


	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	protected List<ViewModels.Document>? documentList;
	protected SuperDataGrid<ViewModels.Document> grid = default!;
	protected DocumentListFilter filter = new();
	protected ViewModels.Document? documentToUpdate;
	protected List<BrokenRule> brokenRules = new();

	protected async Task LoadDatas()
	{
		filter.MetaEntityIdList = new MetaEntityIdList(MetaEntity, new List<Guid> { EntityId });
		filter.PageSize = int.MaxValue;
		var page = await Mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(filter));
		var vmList = Mapping.Profile.ToViewModels(page.List);

		int rowIndex = 1;
		foreach (var item in vmList)
		{
			item.RowIndex = rowIndex++;
		}
		documentList = vmList;
	}

	protected async ValueTask<GridItemsProviderResult<ViewModels.Document>> LoadItems(GridItemsProviderRequest<ViewModels.Document> request)
	{
		if (documentList is null)
        {
            await LoadDatas();
        }

        IEnumerable<ViewModels.Document> rows = documentList ?? [];
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"FileName" => descending ? rows.OrderByDescending(x => x.FileName) : rows.OrderBy(x => x.FileName),
			"Description" => descending ? rows.OrderByDescending(x => x.Description) : rows.OrderBy(x => x.Description),
			"LastUpdate" => descending ? rows.OrderByDescending(x => x.LastUpdate) : rows.OrderBy(x => x.LastUpdate),
			"Size" => descending ? rows.OrderByDescending(x => x.Size) : rows.OrderBy(x => x.Size),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		var items = rows.ToList();
		return GridItemsProviderResult<ViewModels.Document>.From(items.Skip(request.StartIndex).Take(request.Count ?? items.Count).ToList(), items.Count);
	}

	protected async Task ReloadItems()
	{
		documentList = null;
		await grid.ReloadAsync();
	}

	protected async Task EditRow(ViewModels.Document doc)
	{
		documentToUpdate = doc;
		await grid.BeginEditAsync(doc);
	}

	protected async Task SaveRow(ViewModels.Document doc)
	{
		var data = Mapping.Profile.ToData(doc);
		var saveResult = await Mediator.Send(new SaveDocumentRequest(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		documentToUpdate = null;
		await grid.EndEditAsync(doc);
		await ReloadItems();
	}

	protected async Task CancelEdit(ViewModels.Document doc)
	{
		documentToUpdate = null;
		await grid.CancelEditAsync(doc);
		await ReloadItems();
	}

	protected async Task DeleteRow(ViewModels.Document doc)
	{
		var dialogResult = await DialogService.Confirm("Détachement", "Confirmez-vous le détachement de ce document à cette entrée ?");
		if (!dialogResult)
		{
			return;
		}

		var removeResult = await Mediator.Send(new RemoveDocumentFromEntryRequest(EntityId, doc.Id));
		if (removeResult.HasError)
		{
			await NotificationService.NotifyError(removeResult);
		}
		await ReloadItems();
	}

	public async Task InsertRow()
	{
		var result = await DialogService.OpenAsync<Dialogs.AddOrAttachDocumentToEntityDialog>("Ajouter ou selectionner un document",
			options: new DialogOptions
			{

				Width = "800px"
			});

		if (result is null)
		{
			return;
		}

		var documentList = result as IEnumerable<ViewModels.Document>;
		if (documentList is null)
		{
			return;
		}

		foreach (var item in documentList)
		{
			if (MetaEntity == MetaEntity.Entry)
			{
				var documentData = Mapping.Profile.ToData(item);
				var attachResult = await Mediator.Send(new AttachDocumentToEntryRequest(EntityId, documentData));
				if (attachResult.HasError)
				{
					await NotificationService.NotifyError(attachResult);
					break;
				}
			}
		}

		await ReloadItems();
	}
}
