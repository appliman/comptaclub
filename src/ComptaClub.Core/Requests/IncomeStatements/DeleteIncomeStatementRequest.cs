using ComptaClub.Results;

namespace ComptaClub.Requests.IncomeStatements;
public record DeleteIncomeStatementRequest(Guid IncomeStatementId) : IRequest<CommandResult>;
