using System;
using System.Net;

namespace SortMyStuffAPI.Infrastructure
{
    public static class ResultFactory
    {
        public static IResult<T> Success<T>(
            T resultObject = default(T))
        =>
            new DefaultResult<T>
            {
                Succeeded = true,
                ResultObject = resultObject
            };

        public static IResult Success(
            object resultObject = null)
        =>
            new DefaultResult
            {
                Succeeded = true,
                ResultObject = resultObject
            };

        public static IResult<T> Failure<T>(
            string errorMessage = null,
            object errorObject = null)
        =>
            new DefaultResult<T>
            {
                Succeeded = false,
                ErrorObject = errorObject,
                ErrorMessage = errorMessage
            };

        public static IResult Failure(
            string errorMessage = null,
            object errorObject = null)
        =>
            new DefaultResult
            {
                Succeeded = false,
                ErrorObject = errorObject,
                ErrorMessage = errorMessage
            };

        public static IStatusCodeResult<T> SuccessCode<T>(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            T resultObject = default(T))
        =>
            new DefaultStatusCodeResult<T>
            {
                Succeeded = true,
                StatusCode = statusCode,
                ResultObject = resultObject
            };

        public static IStatusCodeResult SuccessCode(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            object resultObject = null)
        =>
            new DefaultStatusCodeResult
            {
                Succeeded = true,
                StatusCode = statusCode,
                ResultObject = resultObject
            };

        public static IStatusCodeResult<T> FailureCode<T>(
            HttpStatusCode statusCode = HttpStatusCode.BadRequest,
            string errorMessage = null,
            object errorObject = null)
        =>
            new DefaultStatusCodeResult<T>
            {
                Succeeded = false,
                StatusCode = statusCode,
                ErrorObject = errorObject,
                ErrorMessage = errorMessage
            };

        public static IStatusCodeResult FailureCode(
            HttpStatusCode statusCode = HttpStatusCode.BadRequest,
            string errorMessage = null,
            object errorObject = null)
        =>
            new DefaultStatusCodeResult
            {
                Succeeded = false,
                StatusCode = statusCode,
                ErrorObject = errorObject,
                ErrorMessage = errorMessage
            };

        public static IStatusCodeResult AddStatusCode(
            this IResult result,
            HttpStatusCode statusCode)
        => 
            new DefaultStatusCodeResult
            {
                Succeeded = result.Succeeded,
                ResultObject = result.ResultObject,
                ErrorObject = result.ErrorObject,
                ErrorMessage = result.ErrorMessage,
                StatusCode = statusCode
            };

        public static IStatusCodeResult<T> AddStatusCode<T>(
            this IResult<T> result,
            HttpStatusCode statusCode)
        =>
            new DefaultStatusCodeResult<T>
            {
                Succeeded = result.Succeeded,
                ResultObject = result.ResultObject,
                ErrorObject = result.ErrorObject,
                ErrorMessage = result.ErrorMessage,
                StatusCode = statusCode
            };

        public static IStatusCodeResult<T> AddFailureStatusCode<T>(
            this IResult result,
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        =>
            result.Succeeded ? 
            throw new InvalidOperationException(
                "The result cannot be successful") :
            new DefaultStatusCodeResult<T>
            {
                Succeeded = false,
                ErrorObject = result.ErrorObject,
                ErrorMessage = result.ErrorMessage,
                StatusCode = statusCode
            };

        public static IStatusCodeResult AddFailureStatusCode(
            this IResult result,
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        =>
            result.Succeeded ?
            throw new InvalidOperationException(
                "The result cannot be successful") :
            new DefaultStatusCodeResult
            {
                Succeeded = false,
                ErrorObject = result.ErrorObject,
                ErrorMessage = result.ErrorMessage,
                StatusCode = statusCode
            };

        public static IResult ToNonGeneric<T>(
            this IResult<T> result)
        =>
            new DefaultResult
            {
                Succeeded = result.Succeeded,
                ResultObject = result.ResultObject,
                ErrorObject = result.ErrorObject,
                ErrorMessage = result.ErrorMessage,
            };

        public static IStatusCodeResult ToNonGeneric<T>(
            this IStatusCodeResult<T> result)
        =>
            new DefaultStatusCodeResult
            {
                Succeeded = result.Succeeded,
                ResultObject = result.ResultObject,
                ErrorObject = result.ErrorObject,
                ErrorMessage = result.ErrorMessage,
                StatusCode = result.StatusCode
            };

        private class DefaultResult<T> : IResult<T>
        {
            public bool Succeeded { get; set; }

            public T ResultObject { get; set; }

            public object ErrorObject { get; set; }

            public string ErrorMessage { get; set; }
        }

        private class DefaultResult :
            DefaultResult<object>,
            IResult
        {
        }

        private class DefaultStatusCodeResult<T>
            : DefaultResult<T>,
            IStatusCodeResult<T>
        {
            public HttpStatusCode StatusCode { get; set; }
        }

        private class DefaultStatusCodeResult
            : DefaultStatusCodeResult<object>,
            IStatusCodeResult
        {
        }
    }
}
