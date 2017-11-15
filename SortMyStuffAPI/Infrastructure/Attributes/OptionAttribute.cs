using System;

namespace SortMyStuffAPI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        private readonly Type _enumType;

        public OptionAttribute(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(
                    "Option attribute must take in an Enum type.");
            }

            _enumType = enumType;
        }

        public Array Options { get => Enum.GetValues(_enumType); }
    }
}
