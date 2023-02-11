
using AutoMapper;

using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Configuration;
using ComptaClub.Models;
using ComptaClub.Requests;

using DocumentFormat.OpenXml.Presentation;

using MediatR;

using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace ComptaClub.Blazor.Pages;

public partial class MemberList : ComponentBase
{
    [CascadingParameter]
    Shared.MainLayout MainLayout { get; set; } = default!;

    [Inject]
    IMediator Mediator { get; set; } = default!;

    [Inject]
    IMapper Mapper { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    List<ViewModels.Member>? memberList;
    RadzenDataGrid<ViewModels.Member>? grid = new();
    MemberListFilter filter = new();
    bool displayUpload = false;

    protected override void OnInitialized()
    {
        MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
        {
            IconName = "upload_file",
            Text = "Importer",
            Title = "Importer à partir d'un fichier excel",
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
        var page = await Mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(filter));
        memberList = Mapper.Map<List<ViewModels.Member>>(page.List);

        var balanceByMemberList = await Mediator.Send(new Requests.Members.GetBalanceByMemberListRequest(filter)); 
        int rowIndex = 1;
        foreach (var item in memberList)
        {
            var balance = balanceByMemberList.SingleOrDefault(i => i.MemberId == item.Id);
            if (balance != null)
            {
                item.Amount = (balance.Balance / 1000000m);
            }
            item.RowIndex = rowIndex++;
        }
    }

    async Task LoadFile(InputFileChangeEventArgs args)
    {
        var ms = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(ms);
        var dataList = await Mediator.Send(new Requests.Members.ImportExcelMemberListRequest(ms));
        memberList = Mapper.Map<List<ViewModels.Member>>(dataList);
        int rowIndex = 1;
        foreach (var item in memberList)
        {
            item.RowIndex = rowIndex++;
        }
        displayUpload = false;
    }
}