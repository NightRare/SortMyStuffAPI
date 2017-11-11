using System;
using System.Runtime.Serialization;

namespace SortMyStuffAPI.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException()
        { }

        public ApiException(string message) : base(message)
        { }

        public ApiException(string messsage, Exception innerException) 
            : base(messsage, innerException)
        { }

        protected ApiException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
