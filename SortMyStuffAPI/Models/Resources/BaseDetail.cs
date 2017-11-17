using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class BaseDetail : EntityResource
    {
        [Searchable]
        [Sortable]
        public string Label { get; set; }

        public string Style { get; set; }

        [Searchable]
        [Sortable]
        public int Position { get; set; }

        public Link Category { get; set; }

        public Link Derivatives { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link FormSpecs { get; set; }

        [JsonIgnore]
        [Searchable]
        public string CategoryId { get; set; }
    }
}
