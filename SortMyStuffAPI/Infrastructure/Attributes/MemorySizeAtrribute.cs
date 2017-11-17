using SortMyStuffAPI.Exceptions;
using SortMyStuffAPI.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SortMyStuffAPI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemorySizeAttribute : ValidationAttribute
    {
        public Int64 MaximumSize { get; set; }
        public Int64 MinimumSize { get; set; }

        public MemorySizeAttribute()
            :base(ApiStrings.ErrorMemorySizeAttributeMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            EnsureLegalSize();

            return String.Format(
                CultureInfo.CurrentCulture, 
                ErrorMessageString, 
                name, 
                MaximumSize, 
                MaximumSize);
        }

        public override bool IsValid(object value)
        {
            EnsureLegalSize();

            Int64 size = 0;
            if(value != null)
            {
                using(var s = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(s, value);
                    size = s.Length;
                }
            }

            return size >= MinimumSize && size <= MaximumSize;
        }

        private void EnsureLegalSize()
        {
            if(MaximumSize < 0)
            {
                throw new ApiInvalidAttributeException(
                    $"The max size of {nameof(MemorySizeAttribute)} cannot be less than 0.");
            }

            if(MaximumSize < MinimumSize)
            {
                throw new ApiInvalidAttributeException(
                    $"The max size of {nameof(MemorySizeAttribute)} cannot be less than the min size.");
            }
        }
    }
}
