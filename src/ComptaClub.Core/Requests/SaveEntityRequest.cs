
namespace ComptaClub.Requests;

public record SaveEntityRequest<T> : IRequest<Results.PersistResult>
    where T : class, Datas.IPrimaryKey
{
    public SaveEntityRequest(T entity, bool bypassRules = false)
    {
        this.Entity = entity;
        this.BypassRules = bypassRules;
    }

    public T Entity { get; init; }
    public bool BypassRules { get; set; } = false;
}
