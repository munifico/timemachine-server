using System;
using TimeMachineServer.Helper;

namespace TimeMachineServer
{
    public struct YearOfWeek
    {
        private int _year;
        private int _month;
        private int _week;

        public YearOfWeek(DateTime dateTime)
        {
            _year = dateTime.Year;
            _month = dateTime.Month;
            _week = DateTimeHelper.GetWeekOfYear(dateTime);
        }

        public static bool operator <(YearOfWeek lhs, YearOfWeek rhs)
        {
            if (lhs._year < rhs._year)
            {
                return true;
            }
            else if (lhs._year == rhs._year && lhs._month < rhs._month)
            {
                return true;
            }
            else if (lhs._year == rhs._year && lhs._month == rhs._month && lhs._week < rhs._week)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator >(YearOfWeek lhs, YearOfWeek rhs)
        {
            return !(lhs < rhs);
        }

        public static bool operator <=(YearOfWeek lhs, YearOfWeek rhs)
        {
            return (lhs < rhs || lhs == rhs);
        }

        public static bool operator >=(YearOfWeek lhs, YearOfWeek rhs)
        {
            return (lhs > rhs || lhs == rhs);
        }

        public static bool operator ==(YearOfWeek lhs, YearOfWeek rhs)
        {
            return (lhs._year == rhs._year && lhs._month == rhs._month && lhs._week == rhs._week);
        }

        public static bool operator !=(YearOfWeek lhs, YearOfWeek rhs)
        {
            return !(lhs == rhs);
        }
    }
}
