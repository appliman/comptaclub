
namespace ComptaClub.Handlers;

internal class CreateBankRequestHandler : IRequestHandler<Requests.CreateBankRequest, Datas.BankData>
{
    public Task<Datas.BankData> Handle(Requests.CreateBankRequest request, CancellationToken cancellationToken)
    {
        var result = new Datas.BankData();
        result.Id = Guid.NewGuid();
        result.Code = request.Code;
        result.Label = request.Label;
        result.CreationDate = DateTime.Today.ToDayId();
        return Task.FromResult(result);
    }
}
