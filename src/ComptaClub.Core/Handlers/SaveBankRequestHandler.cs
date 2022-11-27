namespace ComptaClub.Handlers
{
    public class SaveBankRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.BankData>, Results.PersistResult<Guid>>
    {
        private readonly IValidator<Datas.BankData> _validator;

        public SaveBankRequestHandler(
            IValidator<Datas.BankData> validator,
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
            ILogger<SaveBankRequestHandler> logger)
            : base(dbContextFactory, logger)
        {
            _validator = validator;
        }

        public async Task<Results.PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Datas.BankData> request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }

            return await SaveEntity<Datas.BankData>(request.Entity);
        }
    }
}
