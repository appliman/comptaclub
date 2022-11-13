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
    public class GetExerciceByCode : IRequestHandler<Requests.GetExerciceByCode, Models.Exercice>
    {
        private readonly IMapper _mapper;
        private readonly ITableStorageService _tableStorageService;

        public GetExerciceByCode(
            AutoMapper.IMapper mapper,
            ITableStorageService tableStorageService)
        {
            _mapper = mapper;
            _tableStorageService = tableStorageService;
        }

        public async Task<Exercice> Handle(Requests.GetExerciceByCode request, CancellationToken cancellationToken)
        {
            var table = await _tableStorageService.GetTable<Datas.Exercice>();

            var data = await table.GetFirstOrDefaultEntity<Datas.Exercice>(f => f.PartitionKey == request.Code);

            var result = _mapper.Map<Models.Exercice>(data);
            return result;
        }
    }
}
