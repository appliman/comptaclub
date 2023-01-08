
using MediatR;

namespace ComptaClub.Blazor.Pages;

public partial class ImportEntryList : ComponentBase
{
    [Inject]
    IMediator Mediator { get; set; } = default!;

    async Task LoadFile(InputFileChangeEventArgs args)
    {
        var ms = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(ms);
        var entryList = await Mediator.Send(new Requests.ImportEntryListFromStreamRequest(ms));
    }
}