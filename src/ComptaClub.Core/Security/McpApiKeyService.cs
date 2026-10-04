using ComptaClub.Contracts.Models.ApiKeys;

namespace ComptaClub.Security;

public sealed class McpApiKeyService(
    IComptaClubDbContextFactory dbContextFactory,
    ICurrentApplicationUser currentUser,
    IValidator<McpApiKeyData> validator,
    TimeProvider timeProvider,
    ILogger<McpApiKeyService> logger)
{
    public async Task<McpApiKeyResult> Create(CreateMcpApiKeyRequest request, CancellationToken cancellationToken)
    {
        var _actor = await RequireUser(cancellationToken);
        var _generated = McpApiKeyGenerator.Generate();
        var _key = new McpApiKeyData
        {
            Id = Guid.NewGuid(), Name = request.Name?.Trim() ?? string.Empty, KeyIdentifier = _generated.Identifier,
            SecretHash = _generated.Hash, SecretLastFour = _generated.LastFour,
            CreatedByUserId = _actor, CreationDateUtc = timeProvider.GetUtcNow().UtcDateTime,
            ExpirationDateUtc = request.ExpirationDateUtc, Version = Guid.NewGuid()
        };
        var _result = await Check(_key, cancellationToken);
        if (_result.HasError)
        {
            return _result;
        }
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        _db.McpApiKeys.Add(_key);
        _result.ChangeCount = await _db.SaveChangesAsync(cancellationToken);
        _result.Id = _key.Id;
        _result.ApiKey = ToInfo(_key, await CreatorName(_db, _actor, cancellationToken));
        _result.PlainTextKey = _generated.PlainText;
        return _result;
    }

    public async Task<McpApiKeyResult> Update(UpdateMcpApiKeyRequest request, CancellationToken cancellationToken)
    {
        await RequireUser(cancellationToken);
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _key = await _db.McpApiKeys.AsTracking().SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        var _result = CheckEditable(_key, request.ExpectedVersion);
        if (_result.HasError)
        {
            return _result;
        }
        _key!.Name = request.Name?.Trim() ?? string.Empty;
        _key.ExpirationDateUtc = request.ExpirationDateUtc;
        _result = await Check(_key, cancellationToken);
        if (_result.HasError)
        {
            return _result;
        }
        return await Save(_db, _key, cancellationToken);
    }

    public async Task<McpApiKeyResult> Rotate(RotateMcpApiKeyRequest request, CancellationToken cancellationToken)
    {
        await RequireUser(cancellationToken);
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _key = await _db.McpApiKeys.AsTracking().SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        var _result = CheckEditable(_key, request.ExpectedVersion);
        if (_result.HasError)
        {
            return _result;
        }
        if (_key!.ExpirationDateUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            _result.AddErrorBrokenRule("ExpirationDateUtc", "Modifiez l’expiration avant de renouveler cette clé.");
            return _result;
        }
        var _generated = McpApiKeyGenerator.Generate();
        _key.KeyIdentifier = _generated.Identifier;
        _key.SecretHash = _generated.Hash;
        _key.SecretLastFour = _generated.LastFour;
        _result = await Save(_db, _key, cancellationToken);
        if (!_result.HasError)
        {
            _result.PlainTextKey = _generated.PlainText;
        }
        return _result;
    }

    public async Task<McpApiKeyResult> Revoke(Guid id, bool archive, CancellationToken cancellationToken)
    {
        await RequireUser(cancellationToken);
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _now = timeProvider.GetUtcNow().UtcDateTime;
        var _count = await _db.McpApiKeys.Where(item => item.Id == id)
            .ExecuteUpdateAsync(update => update
                .SetProperty(item => item.RevokedDateUtc, item => item.RevokedDateUtc ?? _now)
                .SetProperty(item => item.ArchivedDateUtc, item => archive ? item.ArchivedDateUtc ?? _now : item.ArchivedDateUtc)
                .SetProperty(item => item.Version, Guid.NewGuid()), cancellationToken);
        var _result = new McpApiKeyResult { Id = id, ChangeCount = _count };
        if (_count == 0)
        {
            _result.AddErrorBrokenRule("Id", "Clé API introuvable.");
        }
        return _result;
    }

    public async Task<McpApiKeyInfo?> Get(Guid id, CancellationToken cancellationToken)
    {
        await RequireUser(cancellationToken);
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _key = await _db.McpApiKeys.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return _key is null ? null : ToInfo(_key, await CreatorName(_db, _key.CreatedByUserId, cancellationToken));
    }

    public async Task<PagedList<IEnumerable<McpApiKeyInfo>>> List(ListMcpApiKeysRequest request, CancellationToken cancellationToken)
    {
        await RequireUser(cancellationToken);
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _query = _db.McpApiKeys.AsNoTracking().Where(item => request.IncludeArchived || item.ArchivedDateUtc == null);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            _query = _query.Where(item => item.Name.Contains(request.Search));
        }
        var _count = await _query.CountAsync(cancellationToken);
        var _size = Math.Clamp(request.PageSize, 1, 100);
        var _keys = await _query.OrderByDescending(item => item.CreationDateUtc).ThenBy(item => item.Id)
            .Skip(checked(Math.Max(0, request.PageIndex) * _size)).Take(_size).ToListAsync(cancellationToken);
        var _ids = _keys.Select(item => item.CreatedByUserId).ToList();
        var _names = await _db.Users.Where(item => _ids.Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        return new PagedList<IEnumerable<McpApiKeyInfo>>
        {
            List = _keys.Select(item => ToInfo(item, _names.GetValueOrDefault(item.CreatedByUserId, "Utilisateur supprimé"))).ToList(),
            Total = new PagedTotal { RowCount = _count }
        };
    }

    public async Task<McpApiKeyData?> Validate(string suppliedKey, CancellationToken cancellationToken)
    {
        if (!McpApiKeyGenerator.TryParse(suppliedKey, out var _identifier, out var _secret))
        {
            return null;
        }
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _now = timeProvider.GetUtcNow().UtcDateTime;
        var _key = await _db.McpApiKeys.SingleOrDefaultAsync(item => item.KeyIdentifier == _identifier
            && item.RevokedDateUtc == null && item.ArchivedDateUtc == null
            && (item.ExpirationDateUtc == null || item.ExpirationDateUtc > _now)
            && _db.Users.Any(user => user.Id == item.CreatedByUserId && user.DisableDate == null), cancellationToken);
        if (_key is null || !McpApiKeyGenerator.FixedTimeEquals(_key.SecretHash, McpApiKeyGenerator.ComputeHash(_secret)))
        {
            return null;
        }
        return _key;
    }

    public async Task RegisterUsage(Guid id, CancellationToken cancellationToken)
    {
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _now = timeProvider.GetUtcNow().UtcDateTime;
        await _db.McpApiKeys.Where(item => item.Id == id).ExecuteUpdateAsync(update => update
            .SetProperty(item => item.LastUsedDateUtc, _now)
            .SetProperty(item => item.UsageCount, item => item.UsageCount + 1), cancellationToken);
    }

    private async Task<Guid> RequireUser(CancellationToken cancellationToken)
    {
        var _id = await currentUser.GetUserId(cancellationToken);
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (_id is null || !await _db.Users.AnyAsync(item => item.Id == _id && item.DisableDate == null, cancellationToken))
        {
            throw new UnauthorizedAccessException("Un utilisateur actif est requis.");
        }
        return _id.Value;
    }

    private async Task<McpApiKeyResult> Check(McpApiKeyData key, CancellationToken cancellationToken)
    {
        var _result = new McpApiKeyResult();
        var _validation = await validator.ValidateAsync(key, cancellationToken);
        foreach (var _failure in _validation.Errors)
        {
            _result.AddErrorBrokenRule(_failure.PropertyName, _failure.ErrorMessage);
        }
        if (key.ExpirationDateUtc is { } _expiry && (_expiry.Kind != DateTimeKind.Utc || _expiry <= timeProvider.GetUtcNow().UtcDateTime))
        {
            _result.AddErrorBrokenRule("ExpirationDateUtc", "L’expiration doit être une date UTC future.");
        }
        return _result;
    }

    private async Task<McpApiKeyResult> Save(ComptaClubDbContext db, McpApiKeyData key, CancellationToken cancellationToken)
    {
        var _result = new McpApiKeyResult { Id = key.Id };
        key.Version = Guid.NewGuid();
        try
        {
            _result.ChangeCount = await db.SaveChangesAsync(cancellationToken);
            _result.ApiKey = ToInfo(key, await CreatorName(db, key.CreatedByUserId, cancellationToken));
        }
        catch (DbUpdateConcurrencyException)
        {
            _result.AddErrorBrokenRule("Version", "La clé a changé. Rechargez la page.");
        }
        catch (DbUpdateException _exception)
        {
            logger.LogError(_exception, "Impossible de sauvegarder la clé MCP {ApiKeyId}.", key.Id);
            throw;
        }
        return _result;
    }

    private static McpApiKeyResult CheckEditable(McpApiKeyData? key, Guid expectedVersion)
    {
        var _result = new McpApiKeyResult();
        if (key is null)
        {
            _result.AddErrorBrokenRule("Id", "Clé API introuvable.");
        }
        else if (key.ArchivedDateUtc != null || key.RevokedDateUtc != null)
        {
            _result.AddErrorBrokenRule("Id", "Une clé révoquée ou archivée ne peut pas être modifiée.");
        }
        else if (key.Version != expectedVersion)
        {
            _result.AddErrorBrokenRule("Version", "La clé a changé. Rechargez la page.");
        }
        return _result;
    }

    private static Task<string> CreatorName(ComptaClubDbContext db, Guid id, CancellationToken cancellationToken) =>
        db.Users.Where(item => item.Id == id).Select(item => item.Name).SingleAsync(cancellationToken);

    private static McpApiKeyInfo ToInfo(McpApiKeyData key, string creator) =>
        new(key.Id, key.Name, $"cc-{key.KeyIdentifier}.…{key.SecretLastFour}", key.CreatedByUserId, creator,
            DateTime.SpecifyKind(key.CreationDateUtc, DateTimeKind.Utc), Utc(key.ExpirationDateUtc),
            Utc(key.RevokedDateUtc), Utc(key.ArchivedDateUtc), Utc(key.LastUsedDateUtc), key.UsageCount, key.Version);

    private static DateTime? Utc(DateTime? date) => date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : null;
}
