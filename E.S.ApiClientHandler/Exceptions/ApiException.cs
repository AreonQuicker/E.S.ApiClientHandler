using System;
using System.Net;

namespace E.S.ApiClientHandler.Exceptions
{   

    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string Url { get;}
        public ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.SeeOther, string url = null, Exception innerException = null)
           : base(message, innerException)
        {
            StatusCode = statusCode;
            Url = url;
        }     
    }
}
