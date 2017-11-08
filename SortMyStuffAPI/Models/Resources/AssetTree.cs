using Newtonsoft.Json;

namespace SortMyStuffAPI.Models
{
    public class AssetTree : Resource
    {
        public string Name { get; set; }

        public AssetTree[] Contents { get; set; }

        [JsonIgnore]
        public string Id { get; set; }
    }
}
