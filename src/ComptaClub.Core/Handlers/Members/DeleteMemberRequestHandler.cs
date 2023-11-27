using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Members;
internal class DeleteMemberRequestHandler : IRequestHandler<DeleteMemberRequest, CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly ILogger<DeleteMemberRequestHandler> _logger;

    public DeleteMemberRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<DeleteMemberRequestHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(DeleteMemberRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var member = await db.Members.FindAsync(request.MemberId, cancellationToken);
        if (member == null)
        {
            return CommandResult.CreateWarningResult("Membre non trouvé");
        }

        db.Members.Remove(member);

        var changeCount = await db.SaveChangesAsync(cancellationToken);
        if (changeCount == 0)
        {
            _logger.LogWarning("Member {email} not deleted", member.Email);
        }
        else
        {
            _logger.LogInformation("Member {email} deleted with success", member.Email);
        }

        var result = new CommandResult
        {
            ChangeCount = changeCount
        };
        return result;
    }
}
