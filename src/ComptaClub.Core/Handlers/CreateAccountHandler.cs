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
    public class CreateAccountHandler : IRequestHandler<Requests.CreateAccount, Models.Account>
    {
        public Task<Account> Handle(CreateAccount request, CancellationToken cancellationToken)
        {
            var result = new Models.Account();
            result.Id = Guid.NewGuid();
            result.Code = request.Code;
            result.Label = request.Label;
            result.LastUpdate = DateTime.UtcNow;
            result.Direction = request.Direction;
            return Task.FromResult(result);
        }
    }
}
