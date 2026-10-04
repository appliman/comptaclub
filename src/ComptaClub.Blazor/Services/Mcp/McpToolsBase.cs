using ComptaClub.Contracts.Models.ApiKeys;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChannelMediator;
using ComptaClub.Contracts.Results;
using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;

namespace ComptaClub.Blazor.Services.Mcp;

public abstract class McpToolsBase(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
{
    protected IMediator Mediator { get; } = mediator;
    protected IComptaClubDbContextFactory DbContextFactory { get; } = dbContextFactory;
    protected ICurrentApplicationUser CurrentUser { get; } = currentUser;
    protected const int MAX_FILE_BYTES = 512 * 1024;
    protected const long MONEY_SCALE = 1000000;
    private static readonly JsonSerializerOptions JSON_OPTIONS = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected async Task<CallToolResult> Run<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await Actor(cancellationToken);
        try
        {
            var _data = await action();
            var _command = _data as CommandResult;
            var _success = _data is not null && _command?.HasError != true;
            var _payload = System.Text.Json.JsonSerializer.SerializeToElement(new
            {
                succeeded = _success, data = _data,
                errors = _command?.ErrorBrokenRuleList,
                warnings = _command?.WarningBrokenRuleList
            }, JSON_OPTIONS);
            return new CallToolResult
            {
                IsError = !_success, StructuredContent = _payload,
                Content = [new TextContentBlock { Text = _payload.GetRawText() }]
            };
        }
        catch (Exception _exception) when (_exception is ArgumentException or FormatException or InvalidDataException or OverflowException or KeyNotFoundException)
        {
            var _payload = System.Text.Json.JsonSerializer.SerializeToElement(new { succeeded = false, error = _exception.Message }, JSON_OPTIONS);
            return new CallToolResult { IsError = true, StructuredContent = _payload, Content = [new TextContentBlock { Text = _payload.GetRawText() }] };
        }
    }

    protected async Task<Guid> Actor(CancellationToken cancellationToken)
    {
        var _id = await CurrentUser.GetUserId(cancellationToken);
        await using var _db = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        if (_id is null || !await _db.Users.AnyAsync(item => item.Id == _id && item.DisableDate == null, cancellationToken))
        {
            throw new UnauthorizedAccessException("Un utilisateur actif est requis.");
        }
        return _id.Value;
    }

    protected Task<PagedList<IEnumerable<T>>> List<T, TFilter>(int pageIndex, int pageSize, string? search, CancellationToken cancellationToken, Action<TFilter>? configure = null)
        where T : class, IPrimaryKey, new()
        where TFilter : class, IListFilter, new()
    {
        var _filter = new TFilter
        {
            PageIndex = Math.Clamp(pageIndex, 0, 5000), PageSize = Math.Clamp(pageSize, 1, 100),
            Search = search, ComputeRowCount = ComputeRowCount.InAllPages, SortByName = nameof(IPrimaryKey.Id)
        };
        if (_filter is ComptaClub.Contracts.Models.Members.MemberListFilter _memberFilter)
        {
            _memberFilter.MemberState = null;
        }
        if (_filter is ComptaClub.Contracts.Models.Users.UserListFilter _userFilter)
        {
            _userFilter.Options.DeletedState = DeletedState.Both;
        }
        configure?.Invoke(_filter);
        return Mediator.Send(new GetPagedEntityListRequest<TFilter, T>(_filter), cancellationToken);
    }

    protected async Task<T> Find<T, TFilter>(Guid id, CancellationToken cancellationToken)
        where T : class, IPrimaryKey, new()
        where TFilter : class, IListFilter, new()
    {
        var _page = await List<T, TFilter>(0, 1, null, cancellationToken, filter => filter.IdList.Add(id));
        return _page.List.SingleOrDefault(item => item.Id == id) ?? throw new KeyNotFoundException("Élément introuvable.");
    }

    protected async Task EnsureWritableEntry(Guid entryId, CancellationToken cancellationToken)
    {
        var _entry = await Mediator.Send(new ComptaClub.Contracts.Models.Entries.GetEntryByFilterRequest(filter => filter.GetById(entryId)), cancellationToken)
            ?? throw new KeyNotFoundException("Écriture introuvable.");
        var _exercice = await Mediator.Send(new ComptaClub.Contracts.Models.Exercices.GetExerciceByFilterRequest(item => item.Id == _entry.ExerciceId), cancellationToken);
        if (_entry.DeletedDate != null || _exercice is null || _exercice.ClosedDate != null || _exercice.ExerciceState == ExerciceState.Closed)
        {
            throw new ArgumentException("Une écriture supprimée ou appartenant à un exercice clos ne peut pas être modifiée.");
        }
    }

    protected static byte[] DecodeFile(string contentBase64)
    {
        if (contentBase64.Length > ((MAX_FILE_BYTES + 2) / 3) * 4)
        {
            throw new ArgumentException("Le fichier dépasse la limite de 512 Kio.");
        }
        var _bytes = Convert.FromBase64String(contentBase64);
        if (_bytes.Length == 0 || _bytes.Length > MAX_FILE_BYTES)
        {
            throw new ArgumentException("Le fichier est vide ou dépasse la limite de 512 Kio.");
        }
        return _bytes;
    }

    protected static string FileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(':') || fileName is "." or "..")
        {
            throw new ArgumentException("Un nom de fichier simple, sans chemin, est requis.");
        }
        return fileName;
    }

    protected static long Money(decimal euros)
    {
        if (euros < 0 || euros * MONEY_SCALE != decimal.Truncate(euros * MONEY_SCALE))
        {
            throw new ArgumentException("Le montant doit être positif avec au maximum six décimales.");
        }
        return checked((long)(euros * MONEY_SCALE));
    }
}
