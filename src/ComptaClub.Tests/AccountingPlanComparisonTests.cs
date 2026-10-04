using ComptaClub.Extensions;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using ChannelMediator;
using ComptaClub.Blazor.Services;
using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ComptaClub.Tests;

[TestClass]
public class AccountingPlanComparisonTests
{
    [TestMethod]
    [DataRow(Enums.AccountDirection.Debit)]
    [DataRow(Enums.AccountDirection.Credit)]
    public async Task ComparesBothExercisesIncludingAccountsUsedInOnlyOneExercise(Enums.AccountDirection direction)
    {
        await using var _app = await TestHelper.CreateWebApplication();
        var _factory = _app.Services.GetRequiredService<IComptaClubDbContextFactory>();
        await using var _db = await _factory.CreateDbContextAsync();
        var _current = CreateExercice("Courant", new DateTime(2026, 1, 1), true);
        var _previous = CreateExercice("Précédent", new DateTime(2025, 1, 1));
        var _older = CreateExercice("Ancien", new DateTime(2024, 1, 1));
        var _future = CreateExercice("Futur", new DateTime(2027, 1, 1));
        _db.Exercices.AddRange(_future, _older, _current, _previous);

        var _root = CreateAccount("60", direction);
        var _parent = CreateAccount("600", direction, _root.Id);
        var _onlyCurrent = CreateAccount("600001", direction, _parent.Id);
        var _onlyPrevious = CreateAccount("600002", direction, _parent.Id);
        var _both = CreateAccount("600003", direction, _parent.Id);
        _db.Accounts.AddRange(_root, _parent, _onlyCurrent, _onlyPrevious, _both);
        _db.Entries.AddRange(
            CreateEntry(_current, _onlyCurrent, 11),
            CreateEntry(_previous, _onlyPrevious, 22),
            CreateEntry(_current, _both, 33),
            CreateEntry(_previous, _both, 44),
            CreateEntry(_older, _onlyPrevious, 99),
            CreateEntry(_future, _onlyPrevious, 88));
        await _db.SaveChangesAsync();

        var _html = await RenderHome(_app.Services.GetRequiredService<IMediator>());
        StringAssert.Contains(_html, "Exercice N-1");
        StringAssert.Contains(_html, "Précédent");
        AssertAmounts(_html, _onlyCurrent.Code, "11,00", "0,00");
        AssertAmounts(_html, _onlyPrevious.Code, "0,00", "22,00");
        AssertAmounts(_html, _both.Code, "33,00", "44,00");
        AssertAmounts(_html, _parent.Code, "44,00", "66,00");
        AssertAmounts(_html, _root.Code, "44,00", "66,00");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task HidesComparisonWhenThereIsNoPreviousExercise(bool includeFuture)
    {
        await using var _app = await TestHelper.CreateWebApplication();
        var _factory = _app.Services.GetRequiredService<IComptaClubDbContextFactory>();
        await using var _db = await _factory.CreateDbContextAsync();
        _db.Exercices.Add(CreateExercice("Courant", new DateTime(2026, 1, 1), true));
        if (includeFuture)
        {
            _db.Exercices.Add(CreateExercice("Futur", new DateTime(2027, 1, 1)));
        }
        await _db.SaveChangesAsync();

        var _html = await RenderHome(_app.Services.GetRequiredService<IMediator>());
        StringAssert.Contains(_html, "Exercice N");
        Assert.IsFalse(_html.Contains("Exercice N-1", StringComparison.Ordinal));
    }

    private static async Task<string> RenderHome(IMediator mediator)
    {
        var _culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            using var _services = new ServiceCollection()
                .AddSingleton(mediator)
                .AddMemoryCache()
                .AddSingleton<NavigationManager, AccountingTestNavigationManager>()
                .AddSingleton<ListFilterQueryStringParametersService>()
                .BuildServiceProvider();
            await using var _renderer = new HtmlRenderer(_services, NullLoggerFactory.Instance);
            return await _renderer.Dispatcher.InvokeAsync(async () =>
            {
                var _output = await _renderer.RenderComponentAsync<ComptaClub.Blazor.Pages.Index>();
                return WebUtility.HtmlDecode(_output.ToHtmlString());
            });
        }
        finally
        {
            CultureInfo.CurrentCulture = _culture;
        }
    }

    private static void AssertAmounts(string html, string code, string current, string previous)
    {
        var _row = Regex.Matches(html, "<tr[^>]*>.*?</tr>", RegexOptions.Singleline)
            .Single(i => Regex.IsMatch(i.Value, $"<b[^>]*>{Regex.Escape(code)}</b>")).Value;
        var _amounts = Regex.Matches(_row, "class=\"price [^\"]*\"[^>]*>(.*?)</div>", RegexOptions.Singleline)
            .Select(i => i.Groups[1].Value.Replace("\u00a0", " ").Trim()).ToArray();
        CollectionAssert.AreEqual(new[] { $"{current} €", $"{previous} €" }, _amounts);
    }

    private static ExerciceData CreateExercice(string code, DateTime start, bool active = false) => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Label = code,
        StartDate = start.ToDayId(),
        EndDate = start.AddYears(1).AddDays(-1).ToDayId(),
        Active = active
    };

    private static AccountData CreateAccount(string code, Enums.AccountDirection direction, Guid? parentId = null) => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Label = code,
        Direction = direction,
        ParentAccountId = parentId
    };

    private static EntryData CreateEntry(ExerciceData exercice, AccountData account, long euros) => new()
    {
        Id = Guid.NewGuid(),
        PartNumber = Guid.NewGuid().ToString(),
        Label = account.Code,
        ExerciceId = exercice.Id,
        AccountId = account.Id,
        AccountDirection = account.Direction,
        ValueDate = exercice.StartDate,
        Amount = euros * 1000000
    };
}
