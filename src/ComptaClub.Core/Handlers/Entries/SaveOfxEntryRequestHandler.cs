using System.Data;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Entries;

internal sealed class SaveOfxEntryRequestHandler(IComptaClubDbContextFactory factory,
    IValidator<EntryData> validator, IMediator mediator) : IRequestHandler<SaveOfxEntryRequest, PersistResult>
{
    public async Task<PersistResult> Handle(SaveOfxEntryRequest request, CancellationToken cancellationToken)
    {
        var _result = new PersistResult { Id = request.Entry.Id };
        if (string.IsNullOrWhiteSpace(request.Entry.ImportId))
        {
            _result.AddErrorBrokenRule("ImportId", "L’identifiant OFX est requis.");
            return _result;
        }
        var _validation = await validator.ValidateAsync(request.Entry, cancellationToken);
        if (!_validation.IsValid)
        {
            return _validation.ToPersistResult()!;
        }
        await using var _db = await factory.CreateDbContextAsync(cancellationToken);
        await using var _transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (await _db.Entries.AnyAsync(item => item.ImportId == request.Entry.ImportId, cancellationToken))
        {
            _result.AddWarningBrokenRule("ImportId", "Cette écriture est déjà importée.");
            _result.ChangeCount = 0;
            return _result;
        }
        _db.Entries.Add(request.Entry);
        _result.ChangeCount = await _db.SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        await mediator.Publish(new EntrySavedNotification { EntryId = request.Entry.Id, ExerciceId = request.Entry.ExerciceId }, cancellationToken);
        return _result;
    }
}
