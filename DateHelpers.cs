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
            // In Azure, you can also set an App Setting called "WEBSITE_TIME_ZONE" to the timezone id you want instead of this application logic
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }
    }
}
