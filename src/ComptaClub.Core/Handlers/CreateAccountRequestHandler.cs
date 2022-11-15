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
    public class CreateAccountRequestHandler : IRequestHandler<Requests.CreateAccountRequest, Models.Account>
    {
        public Task<Account> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var result = new Models.Account();
            result.Id = Guid.NewGuid();
            result.Code = request.Code;
            result.Label = request.Label;
            result.Direction = request.Direction;
            result.CreationDate = DateTime.Today.ToDayId();
            return Task.FromResult(result);
        }
    }
}
