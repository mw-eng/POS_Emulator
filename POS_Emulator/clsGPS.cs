using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace POS_Emulator
{
    public static class GPS
    {
        public static double UTCsecondsOfTheWeek(DateTime date)
        {
            DateTime utc = date.ToUniversalTime();
            int week = (int)utc.DayOfWeek;
            int hh = utc.Hour;
            int mm = utc.Minute;
            int ss = utc.Second;
            int ff = utc.Millisecond;
            return week * 24 * 3600 + hh * 3600 + mm * 60 + ss + ff / 1000.0;
        }

        public static DateTime UTCsecondsOfTheWeek(double sec)
        {
            DateTime now = DateTime.UtcNow;
            now.AddDays(DayOfWeek.Sunday - now.DayOfWeek);
            int day = (int)Math.Truncate(sec / 24 / 3600);
            sec -= day * 24 * 3600;
            int hh = (int)Math.Truncate(sec / 3600);
            sec -= hh * 3600;
            int mm = (int)Math.Truncate(sec / 60);
            sec -= mm * 60;
            int ss = (int)Math.Truncate(sec);
            sec -= ss;
            int tt = (int)Math.Truncate(sec * 1000);
            now.AddDays(day);
            return new DateTime(now.Year, now.Month, now.Day, hh, mm, ss, tt, DateTimeKind.Utc);
        }
    }
}
