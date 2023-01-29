using AutoMapper;

using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class MemberSelectorDialog
{
    [Inject]
    IMediator Mediator { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Inject]
    AutoMapper.IMapper Mapper { get; set; } = default!;

    List<ViewModels.Member>? memberList;
    RadzenDataGrid<ViewModels.Member>? grid = new();
    MemberListFilter filter = new();
    IList<ViewModels.Member>? selectedMembers;

    async Task LoadDatas(LoadDataArgs args)
    {
        var page = await Mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(filter));
        memberList = Mapper.Map<List<ViewModels.Member>>(page.List);
        int rowIndex = 1;
        foreach (var item in memberList)
        {
            item.RowIndex = rowIndex++;
        }
    }

    Task Select()
    {
        if (selectedMembers != null
            && selectedMembers.Any())
        {
            DialogService.Close(selectedMembers.First());
        }
        else
        {
            DialogService.Close();
        }
        return Task.CompletedTask;
    }

}