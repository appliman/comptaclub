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
    public class CreateExerciceHandler : IRequestHandler<Requests.CreateExercice, Models.Exercice>
    {
        private readonly IMapper _mapper;

        public CreateExerciceHandler(AutoMapper.IMapper mapper)
        {
            _mapper = mapper;
        }

        public Task<Exercice> Handle(CreateExercice request, CancellationToken cancellationToken)
        {
            var result = _mapper.Map<Models.Exercice>(request);
            result.Id = Guid.NewGuid();
            result.CreationDate = DateTime.Today.ToDayId();
            return Task.FromResult(result);
        }
    }
}
