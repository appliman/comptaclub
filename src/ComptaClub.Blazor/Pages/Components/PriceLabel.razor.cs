namespace ComptaClub.Blazor.Pages.Components;

public partial class PriceLabel
{
	[Parameter(CaptureUnmatchedValues = true)]
	public Dictionary<string, object> CapturedAttributes { get; set; } = new();

	[Parameter]
    public long? Value { get; set; }

    [Parameter]
    public bool DisplayIf { get; set; } = true;
    [Parameter]
    public string Color { get; set; } = "default";
    MarkupString PriceValue
    {
        get
        {
            return new MarkupString(string.Format("{0:#,##0.00}&nbsp;€", Value.GetValueOrDefault(0) / 1000000m));
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