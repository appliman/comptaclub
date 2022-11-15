using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Services;

using MediatR;

namespace ComptaClub.Handlers
{
    public class GetExerciceByFilterRequestHandler : IRequestHandler<Requests.GetExerciceByFilterRequest, Models.Exercice>
    {
        private readonly IMapper _mapper;
        private readonly ITableStorageService _tableStorageService;

        public GetExerciceByFilterRequestHandler(
            AutoMapper.IMapper mapper,
            ITableStorageService tableStorageService)
        {
            _mapper = mapper;
            _tableStorageService = tableStorageService;
        }

        public async Task<Exercice> Handle(Requests.GetExerciceByFilterRequest request, CancellationToken cancellationToken)
        {
            var table = await _tableStorageService.GetTable<Datas.Exercice>();

            var data = await table.GetFirstOrDefaultEntity<Datas.Exercice>(request.Filter);

            var result = _mapper.Map<Models.Exercice>(data);
            return result;
        }
    }
}
