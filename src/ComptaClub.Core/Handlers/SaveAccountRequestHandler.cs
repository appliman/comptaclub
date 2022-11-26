using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    public class SaveAccountRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Models.Account>, Models.PersistResult<Guid>>
    {
        private readonly IValidator<Models.Account> _validator;

        public SaveAccountRequestHandler(
            IValidator<Models.Account> validator,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
            ILogger<SaveAccountRequestHandler> logger,
            IMapper mapper)
            : base(dbContextFactory, logger, mapper)
        {
            _validator = validator;
        }

        public async Task<PersistResult<Guid>> Handle(SaveEntityRequest<Models.Account> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return await SaveEntity<Datas.Account>(request.Entity);
        }
    }
}
