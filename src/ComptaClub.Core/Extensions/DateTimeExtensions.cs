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

        public static int? ToDayId(this DateTime? date)
        {
            if (date == null)
            {
                return null;
            }
            var baseDate = new DateTime(2000, 1, 1);
            var dayCount = (date.Value - baseDate).TotalDays;
            return Convert.ToInt32(dayCount);
        }

        public static DateTime FromDayId(this int dayId)
        {
            var date = new DateTime(2000,1,1).AddDays(dayId);
            return date;
        }

        public static DateTime? FromDayId(this int? dayId)
        {
            if (dayId == null)
            {
                return null;
            }
            var date = new DateTime(2000, 1, 1).AddDays(dayId.Value);
            return date;
        }


        public static int FirstDateOfCurrentYear(this DateTime date)
        {
            var year = date.Year;   
            return new DateTime(year,1,1).ToDayId();
        }

        public static int LastDateOfCurrentYear(this DateTime date)
        {
			var year = date.Year;
			return new DateTime(year, 12, 31, 23, 59, 59).ToDayId();
		}
	}
}
