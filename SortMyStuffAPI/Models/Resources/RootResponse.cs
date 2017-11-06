namespace SortMyStuffAPI.Models
{
    public class RootResponse : Resource
    {
        public Link AssetTrees { get; set; }

        public Link Assets { get; set; }
    }
}
