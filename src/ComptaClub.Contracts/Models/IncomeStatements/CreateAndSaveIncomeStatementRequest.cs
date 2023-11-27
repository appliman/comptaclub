using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.IncomeStatements;
public record CreateAndSaveIncomeStatementRequest(Guid ExerciceId)
    : IRequest<PersistResult>;