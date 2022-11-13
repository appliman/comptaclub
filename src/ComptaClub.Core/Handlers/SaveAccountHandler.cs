using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using FluentValidation;

using MediatR;

namespace ComptaClub.Handlers
{
    public class SaveAccountHandler : IRequestHandler<Requests.SaveEntity<Models.Account>, Models.PersistResult<Guid>>
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IValidator<Account> _validator;

        public SaveAccountHandler(ITableStorageService tableStorageService,
            IValidator<Models.Account> validator)
        {
            _tableStorageService = tableStorageService;
            _validator = validator;
        }

        public async Task<PersistResult<Guid>> Handle(SaveEntity<Account> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return await _tableStorageService.SaveEntity<Datas.Account>(request.Entity);
        }
    }
}
