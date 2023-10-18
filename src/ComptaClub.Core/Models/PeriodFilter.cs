namespace ComptaClub.Models;

public class PeriodFilter
{
    public string Name { get; set; } = null!;
    public int? FromDayId { get; set; }
    public int? ToDayId { get; set; }
}
