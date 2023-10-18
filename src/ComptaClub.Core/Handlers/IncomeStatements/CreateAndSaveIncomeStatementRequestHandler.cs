using ComptaClub.Extensions;
using ComptaClub.Requests;
using ComptaClub.Requests.Accounts;
using ComptaClub.Requests.IncomeStatements;
using ComptaClub.Results;

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

using Org.BouncyCastle.Asn1.Tsp;

namespace ComptaClub.Handlers.IncomeStatements;
internal class CreateAndSaveIncomeStatementRequestHandler : IRequestHandler<CreateAndSaveIncomeStatementRequest, PersistResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;

    public CreateAndSaveIncomeStatementRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        IMediator mediator)
    {
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
    }

    public async Task<PersistResult> Handle(CreateAndSaveIncomeStatementRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var exercice = await db.Exercices.FindAsync(request.ExerciceId);
        if (exercice == null)
        {
            return PersistResult.CreateInvalidResult("Exercice introuvable");
        }

        if (exercice.ExerciceState != Enums.ExerciceState.Closed)
        {
            return PersistResult.CreateInvalidResult("Cet exercice n'est pas cloturé");
        }

        var plan = await _mediator.Send(new GetPlanRequest(), cancellationToken);

		var incomeStatement = new IncomeStatementData()
        {
            Id = Guid.NewGuid(),
            ExerciceId = exercice.Id,
            CreationDate = DateTime.Now.ToDayId(),
            CreditTotal = 0,
            DebitTotal = 0,
            Description = $"Compte de résultat : {exercice.Label}"
        };
        db.IncomeStatements.Add(incomeStatement);
        db.Entry(incomeStatement).State = EntityState.Added;

        var flatPlan = plan.ToFlatList();

		// Récupération des totaux par compte
		var accountTotalList = await _mediator.Send(new GetAmountTotalByAccountRequest(exercice.Id), cancellationToken);

        var incomeStatementItemList = new List<IncomeStatementItemData>();  
        var direction = Enums.AccountDirection.Credit;
		foreach (var accountTotal in accountTotalList)
        {
            if (accountTotal.Total == 0)
            {
                continue;
            }

			var accountData = flatPlan.Single(i => i.Id == accountTotal.Id);
            direction = accountData.Direction;

            // On recupere la liste des compte jusqu'a la racine
            var parentList = accountData.GetParentList(flatPlan);

            // On remonte le total jusqu'a la racine
            Guid? parentId = null;
            foreach (var parentAccount in parentList.Reverse())
			{
				var incomeStatementItem = incomeStatementItemList.SingleOrDefault(i => i.AccountId == parentAccount.Id);
                if (incomeStatementItem is null)
                {
                    incomeStatementItem = new IncomeStatementItemData()
                    {
                        Id = Guid.NewGuid(),
                        IncomeStatementId = incomeStatement.Id,
                        AccountId = parentAccount.Id,
                        Code = parentAccount.Code,
                        Label = parentAccount.Label,
                        Amount = accountTotal.Total,
                        Direction = parentAccount.Direction
                    };

                    if (parentId is not null)
                    {
                        incomeStatementItem.ParentIncomeStatementItemId = parentId;
                    }
                    parentId = incomeStatementItem.Id;

                    incomeStatementItemList.Add(incomeStatementItem);
					db.IncomeStatementItems.Add(incomeStatementItem);
					db.Entry(incomeStatementItem).State = EntityState.Added;
				}
                else
                {
                    incomeStatementItem.Amount += accountTotal.Total;
                    parentId = incomeStatementItem.Id;
                }
			}
		
            if (direction == Enums.AccountDirection.Credit)
			{
				incomeStatement.CreditTotal += accountTotal.Total;
			}
			else
			{
				incomeStatement.DebitTotal += accountTotal.Total;
			}
		}

		using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var changeCount = await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new PersistResult
            {
                Id = incomeStatement.Id,
                ChangeCount = changeCount,
                HasError = false
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return PersistResult.CreateInvalidResult(ex.Message);
        }
    }
}
