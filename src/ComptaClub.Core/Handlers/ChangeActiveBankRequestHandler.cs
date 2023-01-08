using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs.Models;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

public class ChangeActiveBankRequestHandler : IRequestHandler<Requests.ChangeActiveBankRequest, CommandResult>
{
	private readonly IMediator _mediator;
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public ChangeActiveBankRequestHandler(IMediator mediator,
		IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
	}

	public async Task<CommandResult> Handle(ChangeActiveBankRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync();
		var bankList = await db.Banks.ToListAsync();

		if (!bankList.Any())
		{
			// Ne devrait pas se produire
			return CommandResult.CreateWarningResult("Il n'y a pas de banque à activer ou desactiver");
		}

		if (bankList.Count == 1
			&& !request.Active)
		{
			return CommandResult.CreateWarningResult("Il n'est pas possible de désactiver l'unique banque");
		}

		var bank = bankList.Single(i => i.Id == request.BankId);

		if (request.Active
			&& bank.Active)
		{
			return CommandResult.CreateWarningResult("Cette banque est déjà active");
		}

		await db.Database.BeginTransactionAsync();

		if (request.Active)
		{
			bank.Active = true;
			foreach (var item in bankList)
			{
				if (item.Id == request.BankId)
				{
					continue;
				}
				if (item.Active)
				{
					item.Active = false;
				}
				db.Entry(item).State = EntityState.Modified;
			}
		}
		else
		{
			bank.Active = false;
			foreach (var item in bankList.OrderByDescending(i => i.CreationDate))
			{
				if (item.Id == request.BankId)
				{
					continue;
				}
				if (!item.Active)
				{
					item.Active = true;
				}
				else
				{
					item.Active = false;
				}
				db.Entry(item).State = EntityState.Modified;
			}
		}
		db.Entry(bank).State = EntityState.Modified;
		var changeCount = await db.SaveChangesAsync();
		await db.Database.CommitTransactionAsync();

		return new CommandResult
		{
			ChangeCount = changeCount,
			HasError = false
		};
	}
}
