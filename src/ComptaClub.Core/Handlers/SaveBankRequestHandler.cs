namespace ComptaClub.Handlers
{
    public class SaveBankRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.BankData>, Results.PersistResult<Guid>>
    {
        private readonly IValidator<Datas.BankData> _validator;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

        public SaveBankRequestHandler(
            IValidator<Datas.BankData> validator,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
            ILogger<SaveBankRequestHandler> logger)
            : base(dbContextFactory, logger)
        {
            _validator = validator;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Results.PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Datas.BankData> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            var db = await _dbContextFactory.CreateDbContextAsync();
            var bankCount = await db.Banks.CountAsync();

            // S'il n'y a aucun exercice, le nouveau doit etre actif
            if (bankCount == 0)
            {
                request.Entity.Active = true;
            }

            var saveResult = await SaveEntity<Datas.BankData>(request.Entity);
            if (!saveResult.HasError)
            {
                bankCount = await db.Banks.CountAsync();
                // Si c'est le seul exercice, il doit etre actif impérativement
                if (bankCount == 1
                    && !request.Entity.Active)
                {
                    request.Entity.Active = true;
                    saveResult = await SaveEntity<Datas.BankData>(request.Entity);
                }
            }

            return saveResult;
        }
    }
}
