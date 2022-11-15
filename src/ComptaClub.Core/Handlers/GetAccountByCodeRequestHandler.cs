using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using MediatR;

namespace ComptaClub.Handlers
{
    public class GetAccountByCodeRequestHandler : IRequestHandler<Requests.GetAccountByCodeRequest, Models.Account>
    {
        private readonly IMapper _mapper;
        private readonly ITableStorageService _tableStorageService;

        public GetAccountByCodeRequestHandler(AutoMapper.IMapper mapper,
            ITableStorageService tableStorageService)
        {
            _mapper = mapper;
            _tableStorageService = tableStorageService;
        }

        public async Task<Account> Handle(GetAccountByCodeRequest request, CancellationToken cancellationToken)
        {
            var bankTable = await _tableStorageService.GetTable<Datas.Account>();

            var data = await bankTable.GetFirstOrDefaultEntity<Datas.Account>(f => f.PartitionKey == request.Code);

            var result = _mapper.Map<Models.Account>(data);
            return result;

        }
    }
}
