using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Core;

using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using FluentValidation;

using MediatR;

namespace ComptaClub.Handlers
{
    public class SaveEntryHandler : IRequestHandler<Requests.SaveEntry, Models.PersistResult<Guid>>
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IValidator<Models.Entry> _validator;

        public SaveEntryHandler(ITableStorageService tableStorageService,
            IValidator<Models.Entry> validator)
        {
            _tableStorageService = tableStorageService;
            _validator = validator;
        }

        public async Task<PersistResult<Guid>> Handle(Requests.SaveEntry request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entry);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return PersistResult<Guid>.CreateInvalidResult("not implemented");
        }
    }
}
