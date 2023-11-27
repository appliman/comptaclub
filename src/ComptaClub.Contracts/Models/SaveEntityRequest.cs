using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models;

public record SaveEntityRequest<T> : IRequest<PersistResult>
    where T : class, IPrimaryKey
{
    public SaveEntityRequest(T entity, bool bypassRules = false)
    {
        Entity = entity;
        BypassRules = bypassRules;
    }

    public T Entity { get; init; }
    public bool BypassRules { get; set; } = false;
}
