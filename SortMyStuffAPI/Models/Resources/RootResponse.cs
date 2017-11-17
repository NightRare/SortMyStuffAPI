namespace SortMyStuffAPI.Models
{
    public class RootResponse : Resource
    {
        public Link SignInToken { get; set; }

        public Link Me { get; set; }

        public Link Documentations { get; set; }

        public Link Categories { get; set; }

        public Link Assets { get; set; }

        public Link BaseDetails { get; set; }

        public Link NewGuid { get; set; }

        public Link Users { get; set; }

    }
}
