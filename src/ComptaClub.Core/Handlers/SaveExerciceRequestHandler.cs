using ComptaClub.Configuration;
using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using FluentValidation;

using MediatR;

namespace ComptaClub.Handlers
{
    public class SaveExerciceRequestHandler : IRequestHandler<Requests.SaveEntityRequest<Models.Exercice>, Models.PersistResult<Guid>>
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IValidator<Models.Exercice> _validator;

        public SaveExerciceRequestHandler(ITableStorageService tableStorageService,
            IValidator<Models.Exercice> validator)
        {
            _tableStorageService = tableStorageService;
            _validator = validator;
        }

        public async Task<PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Models.Exercice> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return await _tableStorageService.SaveEntity<Datas.Exercice>(request.Entity);
        }
    }
}
