using ComptaClub.Results;

namespace ComptaClub.Requests.IncomeStatements;
public record CreateAndSaveIncomeStatementRequest(Guid ExerciceId)
    : IRequest<PersistResult>;