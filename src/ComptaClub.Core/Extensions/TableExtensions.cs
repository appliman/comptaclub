using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ComptaClub.Extensions
{
    public static class TableExtensions
    {
        public static async Task<T?> GetFirstOrDefaultEntity<T>(this TableClient tableClient, Expression<Func<T,bool>> filter)
            where T : class, ITableEntity, new()
        {
            var page = tableClient.QueryAsync(filter);
            T? data = null;
            await foreach (var item in page)
            {
                data = item;
                break;
            }
            return data;
        }

    }
}
