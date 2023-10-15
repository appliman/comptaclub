using Azure.Core;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers.Members;

internal class SaveMemberRequestHandler : SaveRequestHandlerBase,
    IRequestHandler<SaveEntityRequest<MemberData>, PersistResult>
{
    private readonly IValidator<MemberData> _validator;

    public SaveMemberRequestHandler(
        IValidator<MemberData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveMemberRequestHandler> logger,
        IMediator mediator)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
    }

    public async Task<PersistResult> Handle(SaveEntityRequest<MemberData> request, CancellationToken cancellationToken)
    {
        if (!request.BypassRules)
        {
            var result = await _validator.ValidateAsync(request.Entity, cancellationToken);
            if (!result.IsValid)
            {
                return result.ToPersistResult()!;
            }
        }

        var saveResult = await SaveEntity<MemberData>(request.Entity, cancellationToken);
        return saveResult;
    }
}
