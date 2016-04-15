using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HyperVStatusMon
{
    public class DateHelpers
    {
        public static DateTime GetLocalDateTime(DateTime dt)
        {
            // TODO: grab desired timezone from config
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }
    }
}
