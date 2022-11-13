using ComptaClub.Configuration;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using FluentValidation;

using MediatR;

namespace ComptaClub.Handlers
{
    public class SaveBankHandler : IRequestHandler<Requests.SaveEntity<Models.Bank>, Models.PersistResult<Guid>>
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IValidator<Bank> _validator;

        public SaveBankHandler(ITableStorageService tableStorageService,
            IValidator<Models.Bank> validator)
        {
            _tableStorageService = tableStorageService;
            _validator = validator;
        }

        public async Task<PersistResult<Guid>> Handle(Requests.SaveEntity<Models.Bank> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return await _tableStorageService.SaveEntity<Datas.Bank>(request.Entity);
        }
    }
}
