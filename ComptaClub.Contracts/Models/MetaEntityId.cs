using ComptaClub.Enums;

namespace ComptaClub.Contracts.Models;
public record MetaEntityId(MetaEntity MetaEntity, Guid EntityId) : IComparable
{
    public int CompareTo(object? obj)
    {
        if (obj is MetaEntityId other)
        {
            return (MetaEntity, EntityId).CompareTo((other.MetaEntity, other.EntityId));
        }
        else
        {
            return -1;
        }
    }
}