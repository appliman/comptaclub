namespace ComptaClub.Blazor.Pages.Components;

public partial class PriceLabel
{
    [Parameter]
    public decimal? Value { get; set; }

    [Parameter]
    public bool DisplayIf { get; set; } = true;
    [Parameter]
    public string Color { get; set; } = "default";
    MarkupString PriceValue
    {
        get
        {
            return new MarkupString(string.Format("{0:#,##0.00}&nbsp;€", Value.GetValueOrDefault(0)));
        }
    }

    protected override void OnParametersSet()
    {
        if (Value.GetValueOrDefault(0) == 0)
        {
            Color = "zero";
        }
    }
}