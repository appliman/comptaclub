using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Azure.Core;

using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ComptaClub.Handlers
{
    public class SaveEntryRequestHandler : IRequestHandler<Requests.SaveEntryRequest, Models.PersistResult<Guid>>
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IValidator<Models.Entry> _validator;
		private readonly ILogger<SaveEntryRequestHandler> _logger;
		private readonly IMediator _mediator;

		public SaveEntryRequestHandler(ITableStorageService tableStorageService,
            IValidator<Models.Entry> validator,
            ILogger<SaveEntryRequestHandler> logger,
            MediatR.IMediator mediator)
        {
            _tableStorageService = tableStorageService;
            _validator = validator;
			_logger = logger;
			_mediator = mediator;
		}

        public async Task<PersistResult<Guid>> Handle(Requests.SaveEntryRequest request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entry);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            var saveResult = await _tableStorageService.SaveEntity<Datas.Entry>(request.Entry, $"{request.Entry.Id}", request.Entry.Id);
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
