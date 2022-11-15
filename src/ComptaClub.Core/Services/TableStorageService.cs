using System.Security.AccessControl;

using Azure.Core;

using ComptaClub.Configuration;
using ComptaClub.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace ComptaClub.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly ComptaClubSettings _settings;
        private readonly IMapper _mapper;
		private readonly IMemoryCache _cache;
		private readonly ILogger<TableStorageService> _logger;

		public TableStorageService(Configuration.ComptaClubSettings settings,
            AutoMapper.IMapper mapper,
            IMemoryCache cache,
            ILogger<TableStorageService> logger)
        {
            _settings = settings;
            _mapper = mapper;
			_cache = cache;
			_logger = logger;
		}

        public async Task<TableClient> GetTable<T>()
        {
            var tableServiceClient = new TableServiceClient(_settings.AzureStorageConnectionString);
            var tableName = typeof(T).Name;

            if (!_cache.TryGetValue(tableName, out var exists))
            {
                _logger.LogTrace("Try to create table {tableName}", tableName);
                await tableServiceClient.CreateTableIfNotExistsAsync(tableName);
                _cache.Set(tableName, 0);
            }
            return tableServiceClient.GetTableClient(tableName);
        }


        public async Task<PersistResult<Guid>> SaveEntity<T>(Models.IEntityKey model)
            where T : class, ITableEntity, new()
        {
            return await SaveEntity<T>(model, model.Code, model.Id);
        }

		public async Task<PersistResult<Guid>> SaveEntity<T>(object model, string partitionKey, Guid rowKey)
			where T : class, ITableEntity, new()
		{
			var table = await GetTable<T>();
            _logger.LogTrace("Try to save entity {rowKey} in table {Name}", rowKey, table.Name);

            var response = await table.GetEntityIfExistsAsync<T>(partitionKey, $"{rowKey}");

			string? error = null;

			if (!response.HasValue)
            {
				_logger.LogTrace("Try to insert new entity {rowKey} in table {Name}", rowKey, table.Name);
				var data = _mapper.Map<T>(model);
                var azResponse = await table.AddEntityAsync(data);
                if (azResponse.IsError)
                {
                    error = azResponse.ReasonPhrase;
                }
            }
            else
            {
				_logger.LogTrace("Try to update new entity {rowKey} in table {Name}", rowKey, table.Name);
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
                Id = rowKey,
                Code = partitionKey
			};

            if (error != null)
            {
                pResult.HasError = true;
                pResult.ErrorBrokenRuleList = new List<Models.BrokenRule>
                {
                    { new Models.BrokenRule("all", error!) }
                };
                _logger.LogValidationFailedResult("Failed to save entity", pResult);
            }
            else
            {
				_logger.LogTrace("Save entity {rowKey} in table {Name} (succes)", rowKey, table.Name);
			}

			return pResult;
        }
    }
}
