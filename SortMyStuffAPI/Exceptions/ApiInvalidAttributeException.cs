using System;
using System.Runtime.Serialization;

namespace SortMyStuffAPI.Exceptions
{
    public class ApiInvalidAttributeException : ApiException
    {
        public ApiInvalidAttributeException()
        { }

        public ApiInvalidAttributeException(string message) 
            : base(message)
        { }

        public ApiInvalidAttributeException(
            string messsage, Exception innerException) 
            : base(messsage, innerException)
        { }

        protected ApiInvalidAttributeException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
