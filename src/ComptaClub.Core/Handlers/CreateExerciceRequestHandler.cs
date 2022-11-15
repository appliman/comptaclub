using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Handlers
{
    public class CreateExerciceRequestHandler : IRequestHandler<Requests.CreateExerciceRequest, Models.Exercice>
    {
        private readonly IMapper _mapper;

        public CreateExerciceRequestHandler(AutoMapper.IMapper mapper)
        {
            _mapper = mapper;
        }

        public Task<Exercice> Handle(CreateExerciceRequest request, CancellationToken cancellationToken)
        {
            var result = _mapper.Map<Models.Exercice>(request);
            result.Id = Guid.NewGuid();
            result.CreationDate = DateTime.Today.ToDayId();
            return Task.FromResult(result);
        }
    }
}
