namespace SortMyStuffAPI.Models
{
    public class RootResponse : Resource
    {
        public Link Documentations { get; set; }

        public Link AssetTrees { get; set; }

        public Link Assets { get; set; }

        public Link NewGuid { get; set; }
    }
}
