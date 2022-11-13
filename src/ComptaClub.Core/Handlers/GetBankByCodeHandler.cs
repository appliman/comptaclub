using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Configuration;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using MediatR;

namespace ComptaClub.Handlers
{
    public class GetBankByCodeHandler : IRequestHandler<Requests.GetBankByCode, Models.Bank>
    {
        private readonly IMapper _mapper;
        private readonly ITableStorageService _tableStorageService;

        public GetBankByCodeHandler(
            AutoMapper.IMapper mapper,
            ITableStorageService tableStorageService)
        {
            _mapper = mapper;
            _tableStorageService = tableStorageService;
        }

        public async Task<Bank> Handle(GetBankByCode request, CancellationToken cancellationToken)
        {
            var bankTable = await _tableStorageService.GetTable<Datas.Bank>();

            var data = await bankTable.GetFirstOrDefaultEntity<Datas.Bank>(f => f.PartitionKey == request.Code);

            var result = _mapper.Map<Models.Bank>(data);
            return result;
        }

    }
}
