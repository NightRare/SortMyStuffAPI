using System.Net;

namespace SortMyStuffAPI.Infrastructure
{
    public interface IStatusCodeResult : 
        IResult,
        IStatusCodeResult<object>
    {
    }

    public interface IStatusCodeResult<T> : IResult<T>
    {
        HttpStatusCode StatusCode { get; }
    }
}
