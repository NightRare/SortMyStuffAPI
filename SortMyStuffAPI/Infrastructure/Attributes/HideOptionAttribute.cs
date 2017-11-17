using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class HideOptionAttribute : Attribute
    {
    }
}
