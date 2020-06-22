using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RflxWindowsCore
{
    public class RWCDateTimeMethods
    {
        public static DayOfWeek startOfWeek = DayOfWeek.Sunday;

        public static readonly string TwentyFourHrsFormat = "HH:mm";
        public static readonly string TwelveHrsFormat = "hh:mm tt";

        public static DateTime getWeekStrtDate(DateTime d)
        {
            DateTime date = StartOfWeek(d);
            return date;
        }

        public static DateTime getWeekEndDate(DateTime d)
        {
            DateTime date = StartOfWeek(d).AddDays(6);
            return date;
        }

        public static DateTime StartOfWeek(DateTime date)
        {
            int diff = date.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return date.AddDays(-1 * diff).Date;
        }
        public static DateTime getMonthStrtDate(DateTime reqDt)
        {
            DateTime date = reqDt;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            return firstDayOfMonth;
        }

        public static DateTime getMonthEndDate(DateTime reqDt)
        {
            DateTime date = reqDt;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            return lastDayOfMonth;
        }

        public static DateTime getWeekStartDateFromWeekNumber(int _weekNumber, int _year)
        {
            DateTime baseDate = new DateTime(_year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - baseDate.DayOfWeek;

            DateTime firstThursday = baseDate.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);

            var weekNum = _weekNumber;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-4);
        }

        public static HashSet<String> getDatesInBetween(DateTime _startDate, DateTime _endDate)
        {
            //Returns HashSet<String> of dates between given dates in yyyyMMdd format 
            HashSet<String> _dates = new HashSet<string>();
            if (_startDate != _endDate)
            {
                while (_startDate <= _endDate)
                {
                    _dates.Add(_startDate.ToString("yyyyMMdd"));
                    _startDate = _startDate.AddDays(1);
                }
            }
            else
            {
                _dates.Add(_startDate.ToString("yyyyMMdd"));
            }
            return _dates;
        }
        public static int getWeekNumberOfYear(DateTime date)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday)
            {
                date = date.AddDays(0);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(Convert.ToDouble(unixTimeStamp)).ToLocalTime();
            return dtDateTime;
        }

        public static String GetRelativeTime(DateTime date)
        {
            DateTime t1 = DateTime.UtcNow.ToUniversalTime();
            DateTime t2 = date.ToUniversalTime();
            var ts = new TimeSpan(t1.Ticks - t2.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            else if (delta < 120)
            {
                return "a minute ago";
            }
            else if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes ago";
            }
            else if (delta < 5400) // 90 * 60
            {
                return "an hour ago";
            }
            else if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours ago";
            }
            else if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            //else if (delta < 2592000) // 30 * 24 * 60 * 60
            //{
            //    return ts.Days + " days ago";
            //}
            //else if (delta < 604800) // 30 * 24 * 60 * 60
            //{
            //    return ts.Days + " days ago";
            //}
            //else if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            //{
            //    int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            //    return months <= 1 ? "one month ago" : months + " months ago";
            //}
            else
            {
                return date.ToString(@"MMM dd HH:mm tt");
            }
        }
        public static String GetDisplayTime(DateTime date)
        {
            return date.ToString(@"MMM dd HH:mm tt");
        }



        
        
        /* Getting time in 12 or 24hrs format from minutes
             * *@author Akshay Jain
             * *@created 17/06/2017
             */
        public static string GetTimeInSpecifiedFormat(int timeInMinuts, bool is24Hr)
        {
            DateTime _today = DateTime.Today;
            _today = _today.AddMinutes(timeInMinuts);
            if (is24Hr)
            {
                return _today.ToString(TwentyFourHrsFormat).ToLower();
            }
            else
            {
                return _today.ToString(TwelveHrsFormat).ToLower();
            }

        }



    }
}
