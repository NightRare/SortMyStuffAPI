using Newtonsoft.Json;

namespace SortMyStuffAPI.Models
{
    public class AssetTree : Resource
    {
        public AssetTree[] Contents { get; set; }
    }
}
