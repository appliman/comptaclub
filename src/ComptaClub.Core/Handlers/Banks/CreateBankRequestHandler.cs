using ComptaClub.Contracts.Models.Banks;

namespace ComptaClub.Handlers.Banks;

internal class CreateBankRequestHandler : IRequestHandler<CreateBankRequest, BankData>
{
    public Task<BankData> Handle(CreateBankRequest request, CancellationToken cancellationToken)
    {
        var result = new BankData();
        result.Id = Guid.NewGuid();
        result.Code = request.Code;
        result.Label = request.Label;
        result.CreationDate = DateTime.Today.ToDayId();
        return Task.FromResult(result);
    }
}
