using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Utils
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Get the string representation of the current utc date and time in ISO-8601 format
        /// </summary>
        /// <returns>The string representation of the current utc date and time in ISO-8601 format</returns>
        public static string UtcNow()
        {
            return DateTime.UtcNow.ToString("o");
        }
    }
}
