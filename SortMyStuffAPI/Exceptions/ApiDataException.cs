using System;
using System.Runtime.Serialization;

namespace SortMyStuffAPI.Exceptions
{
    public class ApiDataException : ApiException
    {
        public ApiDataException()
        { }

        public ApiDataException(string message) : base(message)
        { }

        public ApiDataException(string messsage, Exception innerException) 
            : base(messsage, innerException)
        { }

        protected ApiDataException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
