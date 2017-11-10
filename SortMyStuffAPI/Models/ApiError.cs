using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using SortMyStuffAPI.Utils;
using System.ComponentModel;
using System.Linq;

namespace SortMyStuffAPI.Models
{
    public class ApiError
    {
        public ApiError() { }

        public ApiError(string message)
        {
            Message = message;
        }

        public ApiError(string message, object detail)
        {
            Message = message;
            Detail = detail.ToString();
        }

        public ApiError(ModelStateDictionary modelState)
        {
            Message = "Invalid parameters.";
            Detail = modelState
                .FirstOrDefault(x => x.Value.Errors.Any())
                .Value
                .Errors
                .FirstOrDefault()
                .ErrorMessage;
        }

        public string Message { get; set; }
            = ApiStrings.ErrorCommonMsg;

        public string Detail { get; set; }
            = ApiStrings.ErrorCommonDetail;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("")]
        public string StackTrace { get; set; }
    }
}
