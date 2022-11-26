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
    public class SaveExerciceRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Models.Exercice>, Models.PersistResult<Guid>>
    {
        private readonly IValidator<Models.Exercice> _validator;

        public SaveExerciceRequestHandler(
            IValidator<Models.Exercice> validator, IDbContextFactory<ComptaClubDbContext> dbContextFactory,
            ILogger<SaveBankRequestHandler> logger,
            IMapper mapper)
            : base(dbContextFactory, logger, mapper)
        {
            _validator = validator;
        }

        public async Task<PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Models.Exercice> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return await SaveEntity<Datas.Exercice>(request.Entity);
        }
    }
}
