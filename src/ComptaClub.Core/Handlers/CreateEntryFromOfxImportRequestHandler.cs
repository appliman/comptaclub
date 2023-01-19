using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Configuration;
using ComptaClub.Requests;
using ComptaClub.Results;

using MediatR;

namespace ComptaClub.Handlers;

public class CreateEntryFromOfxImportRequestHandler : IRequestHandler<Requests.CreateEntryFromOfxImportRequest, Datas.EntryData>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public CreateEntryFromOfxImportRequestHandler(IMediator mediator,
        IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Datas.EntryData> Handle(CreateEntryFromOfxImportRequest request, CancellationToken cancellationToken)
    {
        var activeExercice = await _mediator.Send(new GetActiveExerciceRequest());
        var activeBank = await _mediator.Send(new GetActiveBankRequest());

        var entry = await _mediator.Send(new CreateEntryRequest());

        entry.PartNumber = $"{request.OfxTransactionImport.Name}";
        entry.Label = $"Import:{request.OfxTransactionImport.ReferenceNumber}";
        entry.ImportId = request.OfxTransactionImport.TransactionID;
        if (request.OfxTransactionImport.Amount >= 0)
        {
            entry.AccountDirection = AccountDirection.Credit;
        }
        else
        {
            entry.AccountDirection = AccountDirection.Debit;
        }
        entry.AccountId = ComptaClubSettings.ImportAccount;
        entry.Amount = Convert.ToInt64(request.OfxTransactionImport.Amount * 1000000);
        entry.BankId = activeBank!.Id;
        entry.CreationDate = request.OfxTransactionImport.Date.ToDayId();
        entry.ExerciceId = activeExercice!.Id;
        entry.ExtraInfos = request.OfxTransactionImport.Memo;
        entry.PaymentType = ConvertToPaymentType(request.OfxTransactionImport.TransType);
        entry.ValueDate = request.OfxTransactionImport.FundAvaliabilityDate.ToDayId();
        if (entry.ValueDate < 0)
        {
            entry.ValueDate = entry.CreationDate;
        }

        return entry;
    }

    private Datas.PaymentType ConvertToPaymentType(string transType)
    {
        switch (transType)
        {
            case "CREDIT":
            case "INT":
            case "DIV":
            case "ATM":
            case "POS":
            case "XFER":
                return PaymentType.Transfer;
            case "DEBIT":
            case "FEE":
            case "SRVCHG":
            case "DIRECTDEBIT":
                return PaymentType.Debit;
            case "DEPOT":
            case "CASH":
            case "DIRECTDEP":
                return PaymentType.Cash;
            case "CHECK":
            case "PAYMENT":
                return PaymentType.Check;
            default:
                return PaymentType.Import;
        }
    }
}
