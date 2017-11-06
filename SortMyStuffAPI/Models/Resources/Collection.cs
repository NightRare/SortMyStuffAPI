namespace SortMyStuffAPI.Models.Resources
{
    public class Collection<T> : Resource
    {
        public T[] Value { get; set; }
    }
}
