using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;

using MediatR;

namespace ComptaClub.Requests
{
    public record SaveEntityRequest<T> : IRequest<Models.PersistResult<Guid>>
        where T : class, IEntityKey
    {
        public SaveEntityRequest(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; init; }
    }
}
