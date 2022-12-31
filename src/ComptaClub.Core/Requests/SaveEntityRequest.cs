
namespace ComptaClub.Requests;

public record SaveEntityRequest<T> : IRequest<Results.PersistResult<Guid>>
    where T : class, Datas.IPrimaryKey
{
    public SaveEntityRequest(T entity)
    {
        this.Entity = entity;
    }

    public T Entity { get; init; }
}
