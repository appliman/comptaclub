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
	public class GetAllAccountHierarchizedRequestHandler : IRequestHandler<Requests.GetAllAccountHierarchizedRequest, IEnumerable<Models.Account>>
	{
		private readonly IMapper _mapper;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

        public GetAllAccountHierarchizedRequestHandler(
			AutoMapper.IMapper mapper,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory)
		{
			_mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

		public async Task<IEnumerable<Models.Account>> Handle(GetAllAccountHierarchizedRequest request, CancellationToken cancellationToken)
		{
            var db = await _dbContextFactory.CreateDbContextAsync();

            var result = new List<Models.Account>();
			var page = await db.Accounts.ToListAsync();
			foreach (var data in page)
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
