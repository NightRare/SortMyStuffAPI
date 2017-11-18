using System.Net;

namespace SortMyStuffAPI.Infrastructure
{
    public interface IResult : IResult<object>
    {
    }

    public interface IResult<out T>
    {
        bool Succeeded { get; }
        T ResultObject { get; }
        object ErrorObject { get; }
        string ErrorMessage { get; }
    }
}
