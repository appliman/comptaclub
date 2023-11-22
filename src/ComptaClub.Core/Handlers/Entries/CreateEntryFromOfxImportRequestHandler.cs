using ComptaClub.Configuration;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Import.Models;

namespace ComptaClub.Handlers.Entries;

internal class CreateEntryFromOfxImportRequestHandler : IRequestHandler<CreateEntryFromOfxImportRequest, EntryData>
{
	private readonly IMediator _mediator;
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public CreateEntryFromOfxImportRequestHandler(IMediator mediator,
		IDbContextFactory<ComptaClubDbContext> dbContextFactory)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
	}

	public async Task<EntryData> Handle(CreateEntryFromOfxImportRequest request, CancellationToken cancellationToken)
	{
		var activeExercice = await _mediator.Send(new GetActiveExerciceRequest());
		var activeBank = await _mediator.Send(new GetActiveBankRequest());

		var entry = await _mediator.Send(new CreateEntryRequest());

		entry.PartNumber = $"{request.OfxTransactionImport.Name}";
		entry.Label = $"Import:{request.OfxTransactionImport.ReferenceNumber}";
		entry.ImportId = request.OfxTransactionImport.TransactionID;
		if (request.OfxTransactionImport.Amount >= 0)
		{
			entry.AccountDirection = Enums.AccountDirection.Credit;
		}
		else
		{
			entry.AccountDirection = Enums.AccountDirection.Debit;
		}
		entry.AccountId = ComptaClubSettings.ImportAccount;
		entry.Amount = Math.Abs(Convert.ToInt64(request.OfxTransactionImport.Amount * 1000000));
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

	private Enums.PaymentType ConvertToPaymentType(string transType)
	{
		switch (transType)
		{
			case "CREDIT":
			case "INT":
			case "DIV":
			case "ATM":
			case "POS":
			case "XFER":
				return Enums.PaymentType.Transfer;
			case "DEBIT":
			case "FEE":
			case "SRVCHG":
			case "DIRECTDEBIT":
				return Enums.PaymentType.Debit;
			case "DEPOT":
			case "CASH":
			case "DIRECTDEP":
				return Enums.PaymentType.Cash;
			case "CHECK":
			case "PAYMENT":
				return Enums.PaymentType.Check;
			default:
				return Enums.PaymentType.Import;
		}
	}
}
