using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;
using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Handlers
{
	public class GetCurrentBalanceRequestHandler : IRequestHandler<Requests.GetCurrentBalanceRequest, Models.Balance?>
	{
		private readonly IMapper _mapper;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
        private readonly IMediator _mediator;

		public GetCurrentBalanceRequestHandler(AutoMapper.IMapper mapper,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
			MediatR.IMediator mediator)
		{
			_mapper = mapper;
            _dbContextFactory = dbContextFactory;
            _mediator = mediator;
		}

		public async Task<Models.Balance?> Handle(GetCurrentBalanceRequest request, CancellationToken cancellationToken)
		{
            var db = await _dbContextFactory.CreateDbContextAsync();
			var data = await db.Exercices.FirstOrDefaultAsync(i => i.Active);

			var exercice = _mapper.Map<Models.Exercice>(data);
			if (exercice is null) 
			{
				throw new Exception("Il n'y a pas d'exercice en cours");
			}

			if (!exercice.LastEntryId.HasValue)
			{
				return new Balance(exercice.InitialAmount);
			}

			var lastEntry = await _mediator.Send(new GetEntryByIdRequest(exercice.LastEntryId.Value));
			if (lastEntry is null)
			{
				throw new Exception("Ne devrait pas arriver");
			}

			return lastEntry.Balance;
		}
	}
}
