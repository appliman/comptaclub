using Azure.Core;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

internal class SaveMemberRequestHandler : SaveRequestHandlerBase,
    IRequestHandler<Requests.SaveEntityRequest<Datas.MemberData>, Results.PersistResult<Guid>>
{
    private readonly IValidator<MemberData> _validator;

    public SaveMemberRequestHandler(
        IValidator<Datas.MemberData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveMemberRequestHandler> logger,
        MediatR.IMediator mediator)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
    }

    public async Task<PersistResult<Guid>> Handle(SaveEntityRequest<MemberData> request, CancellationToken cancellationToken)
    {
        if (!request.BypassRules)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }
        }

        var saveResult = await SaveEntity<Datas.MemberData>(request.Entity);
        return saveResult;
    }
}
