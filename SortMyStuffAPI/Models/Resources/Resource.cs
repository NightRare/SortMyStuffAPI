using Newtonsoft.Json;

namespace SortMyStuffAPI.Models.Resources
{
    public abstract class Resource : Link
    {
        [JsonIgnore]
        public Link Self { get; set; }
    }
}
