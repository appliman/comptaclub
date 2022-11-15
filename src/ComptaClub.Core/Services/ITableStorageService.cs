using ComptaClub.Models;

namespace ComptaClub.Services
{
    public interface ITableStorageService
    {
        Task<TableClient> GetTable<T>();
        Task<PersistResult<Guid>> SaveEntity<T>(Models.IEntityKey model) where T : class, ITableEntity, new();
		Task<PersistResult<Guid>> SaveEntity<T>(object model, string partitionKey, Guid rowKey) where T : class, ITableEntity, new();

	}
}