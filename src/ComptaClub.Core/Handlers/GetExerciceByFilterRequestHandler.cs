using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;
using ComptaClub.Models;
using ComptaClub.Services;
using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Handlers
{
    public class GetExerciceByFilterRequestHandler : IRequestHandler<Requests.GetExerciceByFilterRequest, Models.Exercice>
    {
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

        public GetExerciceByFilterRequestHandler(
            AutoMapper.IMapper mapper,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Models.Exercice> Handle(Requests.GetExerciceByFilterRequest request, CancellationToken cancellationToken)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();

            var data = await db.Exercices.FirstOrDefaultAsync(request.Filter);

            var result = _mapper.Map<Models.Exercice>(data);
            return result;
        }
    }
}
