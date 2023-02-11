using AutoMapper;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Results;

using MediatR;

namespace ComptaClub.Blazor.Pages;

public partial class DocumentList : ComponentBase
{
    [CascadingParameter]
    Shared.MainLayout MainLayout { get; set; } = default!;

    [Inject]
    IMediator Mediator { get; set; } = default!;

    [Inject]
    IMapper Mapper { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    List<ViewModels.Document>? documentList;
    RadzenDataGrid<ViewModels.Document>? grid = new();
    DocumentListFilter filter = new();
    bool displayUpload = false;
    ViewModels.Document? documentToUpdate;
    List<Results.BrokenRule> brokenRules = new();

    protected override void OnInitialized()
    {
        MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
        {
            IconName = "upload_file",
            Text = "Ajouter",
            Title = "Ajouter un document",
            OnClick = () =>
            {
                displayUpload = !displayUpload;
                StateHasChanged();
                return Task.CompletedTask;
            }
        }).Display();

        filter.PageSize = 100;
    }

    async Task LoadDatas(LoadDataArgs args)
    {
        var page = await Mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(filter));
        var vmList = Mapper.Map<List<ViewModels.Document>>(page.List);

        int rowIndex = 1;
        foreach (var item in vmList)
        {
            item.RowIndex = rowIndex++;
        }
        documentList = vmList;
    }

    async Task LoadFile(InputFileChangeEventArgs args)
    {
        var extension = System.IO.Path.GetExtension(args.File.Name);

        var ms = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(ms);

        var document = await Mediator.Send(new Requests.Documents.CreateDocumentRequest());
        document.FileName = args.File.Name;
        document.MimeType = args.File.ContentType;

        var saveResult = await Mediator.Send(new Requests.Documents.SaveDocumentRequest(document, ms));
        if (!saveResult.HasError)
        {
            await grid!.Reload();
        }


        displayUpload = false;
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
        var saveResult = await Mediator.Send(new Requests.Documents.SaveDocumentRequest(data));
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
        await Mediator.Send(new Requests.Documents.DeleteDocumentRequest(doc.Id));
        await grid!.Reload();
    }

}