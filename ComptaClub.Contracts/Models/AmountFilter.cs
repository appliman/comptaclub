namespace ComptaClub.Contracts.Models;
public class AmountFilter
{
    public long PrecisionInCentime { get; set; } = 1 * 1000000; // 1 euro
    public long Amount { get; set; } = 0;
    public long Min
    {
        get
        {
            return Amount - PrecisionInCentime;
        }
    }

	public long Max
    {
        get
        {
            return Amount + PrecisionInCentime;
        }
    }

}
