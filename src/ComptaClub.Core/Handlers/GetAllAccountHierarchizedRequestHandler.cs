using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using MediatR;

namespace ComptaClub.Handlers
{
	public class GetAllAccountHierarchizedRequestHandler : IRequestHandler<Requests.GetAllAccountHierarchizedRequest, IEnumerable<Models.Account>>
	{
		private readonly IMapper _mapper;
		private readonly ITableStorageService _tableStorageService;

		public GetAllAccountHierarchizedRequestHandler(
			AutoMapper.IMapper mapper,
			ITableStorageService tableStorageService)
		{
			_mapper = mapper;
			_tableStorageService = tableStorageService;
		}

		public async Task<IEnumerable<Account>> Handle(GetAllAccountHierarchizedRequest request, CancellationToken cancellationToken)
		{
			var table = await _tableStorageService.GetTable<Datas.Account>();

			var result = new List<Account>();
			var page = table.QueryAsync<Datas.Account>();
			await foreach (var data in page)
			{
				var account = _mapper.Map<Models.Account>(data!);
				account.Level = account.ParentAccountId == null ? 0 : -1;
				result.Add(account);
			}

			result.Levelize();
			result.Hierarchize();

			return result;
		}
	}
}
