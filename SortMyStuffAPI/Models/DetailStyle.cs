using SortMyStuffAPI.Infrastructure.Attributes;
using System;

namespace SortMyStuffAPI.Models
{
    [Flags]
    public enum DetailStyle
    {
        [HideOption]
        None = 0,

        Text = 1,

        Date = 2
    }
}
