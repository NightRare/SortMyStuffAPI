using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class EntityResource : Resource
    {
        [JsonIgnore]
        public virtual string Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [Searchable]
        public virtual string UserId { get; set; }
    }
}
