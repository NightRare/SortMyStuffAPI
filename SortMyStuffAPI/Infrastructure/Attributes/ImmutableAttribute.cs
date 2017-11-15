using System;

namespace SortMyStuffAPI.Infrastructure
{
    /// <summary>
    /// This attribute applies to the properties of Update Forms.
    /// This attribute should be consistent with the use of <see cref="MutableAttribute"/>.
    /// However, unlike the <see cref="MutableAttribute"/> attribute, this attribute is 
    /// checked before sending the updating request to the data layer. It is mainly for
    /// sending a corresponding error message back to the client user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ImmutableAttribute : Attribute
    {
    }
}
