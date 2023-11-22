using ComptaClub.Enums;

namespace ComptaClub.Contracts.Models;
public record MetaEntityIdList(MetaEntity MetaEntity, IEnumerable<Guid> EntityIdList);