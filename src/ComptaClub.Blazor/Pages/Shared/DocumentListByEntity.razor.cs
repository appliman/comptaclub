using AutoMapper;

using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Results;

using MediatR;

namespace ComptaClub.Blazor.Pages.Shared;

public partial class DocumentListByEntity
{
	[Parameter]
	public MetaEntity MetaEntity { get; set; }

	[Parameter]
	public Guid EntityId { get; set; }

	[Inject]
	IMapper Mapper { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;


	List<ViewModels.Document>? documentList;
	RadzenDataGrid<ViewModels.Document> grid = default!;
	DocumentListFilter filter = new();
	ViewModels.Document? documentToUpdate;
	List<BrokenRule> brokenRules = new();

	async Task LoadDatas(LoadDataArgs args)
	{
		filter.MetaEntityIdList = new MetaEntityIdList(MetaEntity, new List<Guid> { EntityId });
		filter.PageSize = int.MaxValue;
		var page = await Mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(filter));
		var vmList = Mapper.Map<List<ViewModels.Document>>(page.List);

		int rowIndex = 1;
		foreach (var item in vmList)
		{
			item.RowIndex = rowIndex++;
		}
		documentList = vmList;
	}

	async Task EditRow(ViewModels.Document doc)
	{
		documentToUpdate = doc;
		await grid!.EditRow(doc);
	}

	async Task SaveRow(ViewModels.Document doc)
	{
		documentToUpdate = null;

		var data = Mapper!.Map<Datas.DocumentData>(doc);
		var saveResult = await Mediator.Send(new SaveDocumentRequest(data));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid!.UpdateRow(doc);
	}

	void CancelEdit(ViewModels.Document doc)
	{
		documentToUpdate = null;
		grid!.CancelEditRow(doc);
	}

	async Task DeleteRow(ViewModels.Document doc)
	{
		var dialogResult = await DialogService.Confirm("Confirmez-vous le détachement de ce document à cette entrée ?", "Détachement");
		if (dialogResult.Value == false)
		{
			return;
		}

		var removeResult = await Mediator.Send(new RemoveDocumentFromEntryRequest(EntityId, doc.Id));
		if (removeResult.HasError)
		{
			NotificationService.NotifyError(removeResult);
		}
		await grid!.Reload();
	}

	public async Task InsertRow()
	{
		var result = await DialogService.OpenAsync<Dialogs.AddOrAttachDocumentToEntityDialog>("Ajouter ou selectionner un document",
			options: new DialogOptions
			{
				CloseDialogOnEsc = true,
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
				var documentData = Mapper.Map<Datas.DocumentData>(item);
				var attachResult = await Mediator.Send(new AttachDocumentToEntryRequest(EntityId, documentData));
				if (attachResult.HasError)
				{
					NotificationService.NotifyError(attachResult);
					break;
				}
			}
		}

		await grid.Reload();
	}
}