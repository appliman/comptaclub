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
    public class GetEntryByIdRequestHandler : IRequestHandler<Requests.GetEntryByIdRequest, Models.Entry>
    {
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

        public GetEntryByIdRequestHandler(AutoMapper.IMapper mapper,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Models.Entry> Handle(GetEntryByIdRequest request, CancellationToken cancellationToken)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();

            var data = await db.Entries.FirstOrDefaultAsync(f => f.Id == request.Id);

            var result = _mapper.Map<Models.Entry>(data);
            return result;

        }
    }
}
