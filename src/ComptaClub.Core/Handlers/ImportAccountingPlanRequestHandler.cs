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
	internal class ImportAccountingPlanRequestHandler : IRequestHandler<Requests.ImportAccountingPlanRequest, Models.CommandResult>
	{
		private readonly IMapper _mapper;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
        private readonly IMediator _mediator;

		public ImportAccountingPlanRequestHandler(
			AutoMapper.IMapper mapper,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
			MediatR.IMediator mediator)
		{
			_mapper = mapper;
			_dbContextFactory = dbContextFactory;
			_mediator = mediator;
		}

		public async Task<CommandResult> Handle(ImportAccountingPlanRequest request, CancellationToken cancellationToken)
		{
			var list = ToFlatList(request.HierarchizedAccountingPlan);
			foreach (var account in list)
			{
				var saveResult = await _mediator.Send(new Requests.SaveEntityRequest<Models.Account>(account));
				if (saveResult.HasError)
				{
					throw new Exception();
				}
			}

			return new CommandResult();
		}

		private List<Models.Account> ToFlatList(List<Models.Account> plan)
		{
			var result = new List<Models.Account>();
			while (true)
			{
				var item = plan.FirstOrDefault();
				if (item == null)
				{
					break;
				}
				plan.Remove(item);
				result.Add(item);
				if (item.Children.Any())
				{
					var flat = ToFlatList(item.Children);
					result.AddRange(flat);
				}
			}
			return result;
		}
	}
}
