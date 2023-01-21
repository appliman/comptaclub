using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

public class SaveUserRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.UserData>, Results.PersistResult<Guid>>
{
    private readonly IValidator<Datas.UserData> _validator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public SaveUserRequestHandler(
        IValidator<Datas.UserData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveUserRequestHandler> logger)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PersistResult<Guid>> Handle(SaveEntityRequest<UserData> request, CancellationToken cancellationToken)
    {
        if (!request.BypassRules)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }
        }

        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var saveResult = await SaveEntity<Datas.UserData>(request.Entity);

        return saveResult;
    }
}
