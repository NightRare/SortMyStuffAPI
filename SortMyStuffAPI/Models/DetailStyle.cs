using SortMyStuffAPI.Infrastructure;
using System;

namespace SortMyStuffAPI.Models
{
    [Flags]
    public enum DetailStyle
    {
        [Hide]
        None = 0,

        Text = 1,

        Date = 2
    }
}
