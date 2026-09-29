using ComptaClub.Blazor.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ComptaClub.Tests;

[TestClass]
public class ChargesProduitsPieChartRenderingTests
{
    [TestMethod]
    public async Task RendersChargesInRedAndProduitsInGreenWithoutAmountsBelowChart()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(ChargesProduitsPieChart.Charges)] = 100m,
                [nameof(ChargesProduitsPieChart.Produits)] = 200m
            });
            var output = await renderer.RenderComponentAsync<ChargesProduitsPieChart>(parameters);
            return output.ToHtmlString();
        });

        StringAssert.Contains(html, "fill=\"#198754\"");
        StringAssert.Contains(html, "fill=\"#dc3545\"");
        StringAssert.Contains(html, "class=\"pie-swatch charges\"");
        StringAssert.Contains(html, "class=\"pie-swatch produits\"");
        Assert.IsTrue(html.IndexOf("class=\"pie-swatch produits\"", StringComparison.Ordinal) < html.IndexOf("class=\"pie-swatch charges\"", StringComparison.Ordinal));

        var legendHtml = html[(html.IndexOf("</svg>", StringComparison.Ordinal) + 6)..];
        Assert.IsFalse(legendHtml.Contains("100,00", StringComparison.Ordinal));
        Assert.IsFalse(legendHtml.Contains("200,00", StringComparison.Ordinal));
    }
}
