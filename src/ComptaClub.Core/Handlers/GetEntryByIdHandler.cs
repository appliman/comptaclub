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
    public class GetEntryByIdHandler : IRequestHandler<Requests.GetEntryById, Models.Entry>
    {
        private readonly IMapper _mapper;
        private readonly ITableStorageService _tableStorageService;

        public GetEntryByIdHandler(AutoMapper.IMapper mapper,
            ITableStorageService tableStorageService)
        {
            _mapper = mapper;
            _tableStorageService = tableStorageService;
        }

        public async Task<Entry> Handle(GetEntryById request, CancellationToken cancellationToken)
        {
            var table = await _tableStorageService.GetTable<Datas.Entry>();

            var data = await table.GetFirstOrDefaultEntity<Datas.Entry>(f => f.PartitionKey == $"{request.Id}");

            var result = _mapper.Map<Models.Entry>(data);
            return result;

        }
    }
}
