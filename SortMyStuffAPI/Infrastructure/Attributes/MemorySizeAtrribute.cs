using Newtonsoft.Json;
using SortMyStuffAPI.Exceptions;
using SortMyStuffAPI.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SortMyStuffAPI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DataSizeAttribute : ValidationAttribute
    {
        public Int64 MaximumSize { get; set; }
        public Int64 MinimumSize { get; set; }

        public DataSizeAttribute()
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

            int size = 0;

            if(value != null)
            {
                var json = JsonConvert.SerializeObject(value);
                size = Encoding.Unicode.GetByteCount(json);
            }

            return size >= MinimumSize && size <= MaximumSize;
        }

        private void EnsureLegalSize()
        {
            if(MaximumSize < 0)
            {
                throw new ApiInvalidAttributeException(
                    $"The max size of {nameof(DataSizeAttribute)} cannot be less than 0.");
            }

            if(MaximumSize < MinimumSize)
            {
                throw new ApiInvalidAttributeException(
                    $"The max size of {nameof(DataSizeAttribute)} cannot be less than the min size.");
            }
        }
    }
}
