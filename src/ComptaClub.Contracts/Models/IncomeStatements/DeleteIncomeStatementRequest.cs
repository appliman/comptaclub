using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.IncomeStatements;
public record DeleteIncomeStatementRequest(Guid IncomeStatementId) : IRequest<CommandResult>;
