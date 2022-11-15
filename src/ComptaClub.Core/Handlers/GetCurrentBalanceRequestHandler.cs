using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Data.Tables;

using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using MediatR;

namespace ComptaClub.Handlers
{
	public class GetCurrentBalanceRequestHandler : IRequestHandler<Requests.GetCurrentBalanceRequest, Models.Balance?>
	{
		private readonly IMapper _mapper;
		private readonly ITableStorageService _tableStorageService;
		private readonly IMediator _mediator;

		public GetCurrentBalanceRequestHandler(AutoMapper.IMapper mapper,
			ITableStorageService tableStorageService,
			MediatR.IMediator mediator)
		{
			_mapper = mapper;
			_tableStorageService = tableStorageService;
			_mediator = mediator;
		}

		public async Task<Balance?> Handle(GetCurrentBalanceRequest request, CancellationToken cancellationToken)
		{
			var table = await _tableStorageService.GetTable<Datas.Exercice>();
			var page = table.QueryAsync<Datas.Exercice>(i => i.Active);
			Models.Exercice? exercice = null;
			await foreach (var item in page)
			{
				exercice = _mapper.Map<Models.Exercice>(item);
				break;
			}
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
