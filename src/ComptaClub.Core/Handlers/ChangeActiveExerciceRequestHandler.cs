using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs.Models;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

public class ChangeActiveExerciceRequestHandler : IRequestHandler<Requests.ChangeActiveExerciceRequest, CommandResult>
{
	private readonly IMediator _mediator;
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public ChangeActiveExerciceRequestHandler(IMediator mediator,
		IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
	}

	public async Task<CommandResult> Handle(ChangeActiveExerciceRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync();
		var exerciceList = await db.Exercices.ToListAsync();

		if (!exerciceList.Any())
		{
			// Ne devrait pas se produire
			return CommandResult.CreateWarningResult("Il n'y a pas d'exercice a activer ou desactiver");
		}

		if (exerciceList.Count == 1
			&& !request.Active)
		{
			return CommandResult.CreateWarningResult("Il n'est pas possible de désactiver l'unique exercice");
		}

		var exercice = exerciceList.Single(i => i.Id == request.ExerciceId);

		if (request.Active
			&& exercice.Active)
		{
			return CommandResult.CreateWarningResult("Cet exercice est déjà actif");
		}

		await db.Database.BeginTransactionAsync();

		if (request.Active)
		{
			exercice.Active = true;
			foreach (var item in exerciceList)
			{
				if (item.Id == request.ExerciceId)
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
			exercice.Active = false;
			foreach (var item in exerciceList.OrderByDescending(i => i.StartDate))
			{
				if (item.Id == request.ExerciceId)
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
		db.Entry(exercice).State = EntityState.Modified;
		var changeCount = await db.SaveChangesAsync();
		await db.Database.CommitTransactionAsync();

		return new CommandResult
		{
			ChangeCount = changeCount,
			HasError = false
		};
	}
}
