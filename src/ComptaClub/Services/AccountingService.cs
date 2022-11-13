using System.Data;
using System.Reflection;

using AutoMapper;

using Azure;

using ComptaClub.Configuration;

using Microsoft.AspNetCore.Mvc;

namespace ComptaClub.Services
{
    public class AccountingService
    {
        private readonly ComptaClubSettings _settings;
        private readonly IMapper _mapper;

        public AccountingService(Configuration.ComptaClubSettings settings,
            AutoMapper.IMapper mapper)
        {
            _settings = settings;
            _mapper = mapper;
        }
        public async Task<TableClient> GetTable(string tableName)
        {
            var tableServiceClient = new TableServiceClient(_settings.AzureStorageConnectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync(tableName);

            return tableServiceClient.GetTableClient(tableName);
        }

        public Models.Account CreateAccount(string code)
        {
            var result = new Models.Account();
            result.Id = Guid.NewGuid();
            result.Code = code;
            result.CreationDate = DateTime.UtcNow;
            return result;
        }

        public async Task<Models.PersistResult<Guid>> SaveAccount(Models.Account model)
        {
            var validator = new Validators.AccountValidator(this);
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            var accountTable = await GetTable(nameof(Datas.Account));
            var response = await accountTable.GetEntityIfExistsAsync<Datas.Account>(model.Code, $"{model.Id}");
            string? error = null;
            if (!response.HasValue)
            {
                var data = _mapper.Map<Datas.Account>(model);
                var azResponse = await accountTable.AddEntityAsync(data);
                if (azResponse.IsError)
                {
                    error = azResponse.ReasonPhrase;
                }
            }
            else
            {
                var data = response.Value;
                data = _mapper.Map(model, data);
                var azResponse = await accountTable.UpdateEntityAsync(data, data!.ETag);
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

        public async Task<Models.Account> GetAccountByCode(string code)
        {
            var accountTable = await GetTable(nameof(Datas.Account));
            var page = accountTable.QueryAsync<Datas.Account>(f => f.PartitionKey == code);
            Datas.Account? data = null;
            await foreach (var item in page)
            {
                if (item.RowKey == code)
                {
                    data = item;
                    break;
                }
            }
            var result = _mapper.Map<Models.Account>(data);
            return result;
        }


        public Models.Bank CreateBank(string name)
        {
            var result = new Models.Bank();
            result.Id = Guid.NewGuid();
            result.Name = name;
            result.CreationDate = DateTime.UtcNow;

            return result;
        }

        public async Task<Models.Bank> GetBankByName(string name)
        {
            var accountTable = await GetTable(nameof(Datas.Account));

            var data = await accountTable.GetFirstOrDefaultEntity<Datas.Account>(f => f.PartitionKey == name);

            var result = _mapper.Map<Models.Bank>(data);
            return result;
        }

        public async Task<Models.PersistResult<Guid>> SaveBank(Models.Bank model)
        {
            var validator = new Validators.BankValidator(this);
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            var accountTable = await GetTable(nameof(Datas.Bank));
            var response = await accountTable.GetEntityIfExistsAsync<Datas.Bank>(model.Name, $"{model.Id}");
            string? error = null;
            if (!response.HasValue)
            {
                var data = _mapper.Map<Datas.Bank>(model);
                var azResponse = await accountTable.AddEntityAsync(data);
                if (azResponse.IsError)
                {
                    error = azResponse.ReasonPhrase;
                }
            }
            else
            {
                var data = response.Value;
                data = _mapper.Map(model, data);
                var azResponse = await accountTable.UpdateEntityAsync(data, data!.ETag);
                if (azResponse.IsError)
                {
                    error = azResponse.ReasonPhrase;
                }
            }

            var pResult = new Models.PersistResult<Guid>()
            {
                Id = model.Id,
                Code = model.Name
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
