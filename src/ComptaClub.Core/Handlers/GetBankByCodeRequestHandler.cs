using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Configuration;
using ComptaClub.Datas;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;
using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Handlers
{
    public class GetBankByCodeRequestHandler : IRequestHandler<Requests.GetBankByCodeRequest, Models.Bank>
    {
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

        public GetBankByCodeRequestHandler(
            AutoMapper.IMapper mapper,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Models.Bank> Handle(GetBankByCodeRequest request, CancellationToken cancellationToken)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();

            var data = await db.Banks.FirstOrDefaultAsync(f => f.Code == request.Code);

            var result = _mapper.Map<Models.Bank>(data);
            return result;
        }

    }
}
