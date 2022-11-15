using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Models;

using MediatR;

namespace ComptaClub.Handlers
{
    public class CreateBankRequestHandler : IRequestHandler<Requests.CreateBankRequest, Models.Bank>
    {
        public Task<Models.Bank> Handle(Requests.CreateBankRequest request, CancellationToken cancellationToken)
        {
            var result = new Models.Bank();
            result.Id = Guid.NewGuid();
            result.Code = request.Code;
            result.Label = request.Label;
            result.CreationDate = DateTime.Today.ToDayId();
            return Task.FromResult(result);
        }
    }
}
