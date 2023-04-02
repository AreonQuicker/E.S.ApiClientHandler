using System;
using System.Net;

namespace E.S.ApiClientHandler.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(string message, HttpStatusCode? statusCode = null,
            Exception innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode? StatusCode { get; }
    }
}