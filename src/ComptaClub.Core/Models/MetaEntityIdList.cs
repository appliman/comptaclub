using ComptaClub.Enums;

namespace ComptaClub.Models;
public record MetaEntityIdList(MetaEntity MetaEntity, IEnumerable<Guid> EntityIdList);