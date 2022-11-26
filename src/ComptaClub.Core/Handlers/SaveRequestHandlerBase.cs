using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace ComptaClub.Handlers
{
    public abstract class SaveRequestHandlerBase
    {
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        protected SaveRequestHandlerBase(IDbContextFactory<ComptaClubDbContext> dbContextFactory,
            ILogger<SaveRequestHandlerBase> logger,
            AutoMapper.IMapper mapper)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public virtual async Task<PersistResult<Guid>> SaveEntity<T>(Models.IEntityKey model)
            where T : class, new()
        {
            var db = await _dbContextFactory.CreateDbContextAsync();

            _logger.LogTrace("Try to save entity {Id} in table {Name}", model.Id, typeof(T).Name);

            var data = await db.Set<T>().FindAsync(model.Id);

            string? error = null;

            if (data == null)
            {
                _logger.LogTrace("Try to insert new entity {Id} in table {Name}", model.Id, typeof(T).Name);
                data = _mapper.Map<T>(model);
                db.Set<T>().Add(data);
                db.Entry(data).State = EntityState.Added;
            }
            else
            {
                _logger.LogTrace("Try to update new entity {Id} in table {Name}", model.Id, typeof(T).Name);
                data = _mapper.Map(model, data);
                db.Set<T>().Attach(data);
                db.Entry(data).State = EntityState.Modified;
            }

            var changeCount = await db.SaveChangesAsync();

            var pResult = new Models.PersistResult<Guid>();
            var id = data as Datas.IPrimaryKey;
            if (id != null) 
            {
                pResult.Id = id.Id;
            }

            pResult.ChangeCount = changeCount;

            if (error != null)
            {
                pResult.HasError = true;
                pResult.ErrorBrokenRuleList = new List<Models.BrokenRule>
                {
                    { new Models.BrokenRule("all", error!) }
                };
                _logger.LogValidationFailedResult("Failed to save entity", pResult);
            }
            else
            {
                _logger.LogTrace("Save entity {Id} in table {Name} (succes)", model.Id, typeof(T).Name);
            }

            return pResult;
        }

    }
}
