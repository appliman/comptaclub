using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using ComptaClub.Datas;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Handlers
{
    public class GetAccountByCodeRequestHandler : IRequestHandler<Requests.GetAccountByCodeRequest, Models.Account>
    {
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

        public GetAccountByCodeRequestHandler(AutoMapper.IMapper mapper,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Models.Account> Handle(GetAccountByCodeRequest request, CancellationToken cancellationToken)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();

            var data = await db.Accounts.FirstOrDefaultAsync(f => f.Code == request.Code);

            var result = _mapper.Map<Models.Account>(data);
            return result;

        }
    }
}
