using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Members;
public record SaveMemberRequest(Datas.MemberData Entity, bool BypassRules = false)
    : IRequest<PersistResult>;
