using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers.Users;

internal class SaveUserRequestHandler : SaveRequestHandlerBase,
    IRequestHandler<SaveEntityRequest<UserData>, PersistResult<Guid>>
{
    private readonly IValidator<UserData> _validator;

    public SaveUserRequestHandler(
        IValidator<UserData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveUserRequestHandler> logger)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
    }

    public async Task<PersistResult<Guid>> Handle(SaveEntityRequest<UserData> request, CancellationToken cancellationToken)
    {
        if (!request.BypassRules)
        {
            var result = await _validator.ValidateAsync(request.Entity, cancellationToken);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }
        }

        var saveResult = await SaveEntity<UserData>(request.Entity, cancellationToken);

        return saveResult;
    }
}
