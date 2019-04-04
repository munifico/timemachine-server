using System;
using System.Globalization;

namespace TimeMachineServer.Helper
{
    public class DateTimeHelper
    {
        public static int GetWeekOfYear(DateTime sourceDate, CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }

            CalendarWeekRule calendarWeekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            return cultureInfo.Calendar.GetWeekOfYear(sourceDate, calendarWeekRule, firstDayOfWeek);
        }

        public static int GetWeekOfYear(DateTime sourceDate)
        {
            return GetWeekOfYear(sourceDate, null);
        }
    }
}
