using ComptaClub.Configuration;
using ComptaClub.Datas;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

namespace ComptaClub.Handlers
{
    public class SaveBankRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Models.Bank>, Models.PersistResult<Guid>>
    {
        private readonly IValidator<Models.Bank> _validator;

        public SaveBankRequestHandler(
            IValidator<Models.Bank> validator,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
            ILogger<SaveBankRequestHandler> logger,
            IMapper mapper)
            : base(dbContextFactory, logger, mapper)
        {
            _validator = validator;
        }

        public async Task<PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Models.Bank> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return await SaveEntity<Datas.Bank>(request.Entity);
        }
    }
}
