namespace ComptaClub.Models;
public class AmountFilter
{
    public int PrecisionInCentime { get; set; } = 100;
    public long Amount { get; set; } = 0;
    internal long Min
    {
        get
        {
            var amountInCentime = Amount / 10000m;
            var min = Convert.ToInt64((amountInCentime - PrecisionInCentime) * 10000);
            return min;
        }
    }

    internal long Max
    {
        get
        {
            var amountInCentime = Amount / 10000m;
            var min = Convert.ToInt64((amountInCentime + PrecisionInCentime) * 10000);
            return min;
        }
    }

}
