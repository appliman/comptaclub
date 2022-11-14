using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Extensions
{
    public static class DateTimeExtensions
    {
        public static int ToDayId(this DateTime date)
        {
            var baseDate = new DateTime(2000, 1, 1);
            var dayCount = (date - baseDate).TotalDays;
            return Convert.ToInt32(dayCount);
        }

        public static DateTime FromDayId(this int dayId)
        {
            var date = new DateTime(2000,0,0).AddDays(dayId);
            return date;
        }
    }
}
