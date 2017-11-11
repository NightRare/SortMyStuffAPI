using System;
using System.Runtime.Serialization;

namespace SortMyStuffAPI.Exceptions
{
    public class InvalidSearchOperationException : ApiException
    {
        public InvalidSearchOperationException()
        { }

        public InvalidSearchOperationException(string message) 
            : base(message)
        { }

        public InvalidSearchOperationException(
            string messsage, 
            Exception innerException) 
            : base(messsage, innerException)
        { }

        protected InvalidSearchOperationException(
            SerializationInfo info, 
            StreamingContext context)
            : base(info, context) { }
    }
}
