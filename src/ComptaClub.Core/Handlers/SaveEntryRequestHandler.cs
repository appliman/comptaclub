using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Azure.Core;

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
    public class SaveEntryRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntryRequest, Models.PersistResult<Guid>>
    {
        private readonly IValidator<Models.Entry> _validator;
        private readonly IMediator _mediator;

        public SaveEntryRequestHandler(
            IValidator<Models.Entry> validator,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
            ILogger<SaveEntryRequestHandler> logger,
            IMapper mapper,
            MediatR.IMediator mediator)
            : base(dbContextFactory, logger, mapper)
        {
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<PersistResult<Guid>> Handle(Requests.SaveEntryRequest request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entry);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            var saveResult = await SaveEntity<Datas.Entry>(request.Entry);
            if (!saveResult.HasError)
            {
                await _mediator.Publish(new Notifications.EntrySavedNotification() 
                { 
                    EntryId = request.Entry.Id, 
                    ExerciceId = request.Entry.ExerciceId 
                });
            }
            return saveResult;
		}
	}
}
