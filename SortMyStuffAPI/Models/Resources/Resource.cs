using Newtonsoft.Json;

namespace SortMyStuffAPI.Models
{
    public abstract class Resource : Link
    {
        [JsonIgnore]
        public Link Self { get; set; }
    }
}
