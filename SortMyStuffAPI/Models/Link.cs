using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Models
{
    public class Link
    {
        public static Link To(string routeName, object routeValues = null)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = ApiStrings.HTTP_GET,
                Relations = null
            };

        public static Link ToCollection(string routeName, object routeValues = null)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = ApiStrings.HTTP_GET,
                Relations = new string[] { "collection" }
            };

        public static Link ToForm(
            string routeName,
            object routeValues = null,
            string method = ApiStrings.HTTP_POST,
            params string[] relations)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = method,
                Relations = relations
            };

        [JsonProperty(Order = -4)]
        public string Href { get; set; }

        [JsonProperty(Order = -3, NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(ApiStrings.HTTP_GET)]
        public string Method { get; set; }

        [JsonProperty(Order = -2, PropertyName = "rel", NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Relations { get; set; }

        [JsonIgnore]
        public string RouteName { get; set; }

        [JsonIgnore]
        public object RouteValues { get; set; }
    }
}
