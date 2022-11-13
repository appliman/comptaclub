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
    public class CreateBankHandler : IRequestHandler<Requests.CreateBank, Models.Bank>
    {
        public Task<Models.Bank> Handle(Requests.CreateBank request, CancellationToken cancellationToken)
        {
            var result = new Models.Bank();
            result.Id = Guid.NewGuid();
            result.Code = request.Code;
            result.Label = request.Label;
            result.LastUpdate = DateTime.UtcNow;
            return Task.FromResult(result);
        }
    }
}
