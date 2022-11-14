using System.Security.AccessControl;

using Azure.Core;

using ComptaClub.Configuration;
using ComptaClub.Models;

namespace ComptaClub.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly ComptaClubSettings _settings;
        private readonly IMapper _mapper;

        public TableStorageService(Configuration.ComptaClubSettings settings,
            AutoMapper.IMapper mapper)
        {
            _settings = settings;
            _mapper = mapper;
        }

        public async Task<TableClient> GetTable<T>()
        {
            var tableServiceClient = new TableServiceClient(_settings.AzureStorageConnectionString);
            var tableName = typeof(T).Name;
            var response = await tableServiceClient.CreateTableIfNotExistsAsync(tableName);
            return tableServiceClient.GetTableClient(tableName);
        }

        public async Task<PersistResult<Guid>> SaveEntity<T>(Models.IEntityKey model)
            where T : class, ITableEntity, new()
        {
            var table = await GetTable<T>();
            var response = await table.GetEntityIfExistsAsync<T>(model.Code, $"{model.Id}");
            string? error = null;

            if (!response.HasValue)
            {
                var data = _mapper.Map<T>(model);
                var azResponse = await table.AddEntityAsync(data);
                if (azResponse.IsError)
                {
                    error = azResponse.ReasonPhrase;
                }
            }
            else
            {
                var data = response.Value;
                data = _mapper.Map(model, data);
                var azResponse = await table.UpdateEntityAsync(data, data!.ETag);
                if (azResponse.IsError)
                {
                    error = azResponse.ReasonPhrase;
                }
            }

            var pResult = new Models.PersistResult<Guid>()
            {
                Id = model.Id,
                Code = model.Code
            };

            if (error != null)
            {
                pResult.HasError = true;
                pResult.ErrorBrokenRuleList = new List<Models.BrokenRule>
                {
                    { new Models.BrokenRule("all", error!) }
                };
            }

            return pResult;
        }
    }
}
