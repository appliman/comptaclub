using System.Globalization;
using ComptaClub.Blazor.Pages;
using ComptaClub.Contracts.Models.Stats;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ComptaClub.Tests;

[TestClass]
public class BalanceChartRenderingTests
{
    [TestMethod]
    public async Task RendersSvgCoordinatesWithInvariantCultureWhenPageCultureIsFrench()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var points = new List<BalanceByDay>
            {
                new() { Day = new DateTime(2024, 9, 3), BalanceAmount = 200m },
                new() { Day = new DateTime(2024, 9, 1), BalanceAmount = 100m },
                new() { Day = new DateTime(2024, 9, 2), BalanceAmount = 150m }
            };

            using var services = new ServiceCollection().BuildServiceProvider();
            await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);
            var html = await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    [nameof(BalanceChart.Data)] = points
                });
                var output = await renderer.RenderComponentAsync<BalanceChart>(parameters);
                return output.ToHtmlString();
            });

            StringAssert.Contains(html, "M 100 240 L 515 140 L 930 40");
            StringAssert.Contains(System.Net.WebUtility.HtmlDecode(html), "200,00 €");
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [TestMethod]
    public async Task DrawsOneVerticalBoundaryForEachMonthIncludingAcrossYears()
    {
        var points = new List<BalanceByDay>
        {
            new() { Day = new DateTime(2024, 9, 1), BalanceAmount = 100m },
            new() { Day = new DateTime(2025, 1, 1), BalanceAmount = 200m }
        };

        using var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BalanceChart.Data)] = points
            });
            var output = await renderer.RenderComponentAsync<BalanceChart>(parameters);
            return output.ToHtmlString();
        });

        Assert.AreEqual(5, System.Text.RegularExpressions.Regex.Matches(html, "class=\"month-boundary\"").Count);
        Assert.AreEqual(5, System.Text.RegularExpressions.Regex.Matches(html, "class=\"month-label\"").Count);
        StringAssert.Contains(html, "SEPT. 2024");
        StringAssert.Contains(html, "JANV. 2025");
        StringAssert.Contains(html, "transform=\"rotate(-45 100 285)\"");
        StringAssert.Contains(html, "x1=\"930\" y1=\"20\" x2=\"930\" y2=\"260\" class=\"month-boundary\"");
    }
}
