using System.Net;
using E.S.ApiClientHandler.Utils;

namespace E.S.ApiClientHandler.Models
{
    public class ApiResponse
    {
        private readonly bool? _success;

        public ApiResponse(bool? success, string errorMessage, HttpResponseMessageWrapper httpResponseMessage)
        {
            _success = success;
            HttpResponseMessage = httpResponseMessage;
            ErrorMessage = errorMessage;
        }

        public ApiResponse(HttpResponseMessageWrapper httpResponseMessage)
            : this(null, null, httpResponseMessage)
        {
        }

        public ApiResponse(string errorMessage, HttpResponseMessageWrapper httpResponseMessage)
            : this(null, errorMessage, httpResponseMessage)
        {
        }

        public ApiResponse(bool success)
            : this(success, null, null)
        {
        }

        public ApiResponse(bool success, string errorMessage)
            : this(success, errorMessage, null)
        {
        }

        public HttpResponseMessageWrapper HttpResponseMessage { get; set; }

        public bool Success => _success ?? HttpResponseMessage?.IsSuccess ?? false;

        public string ErrorMessage { get; set; }

        public HttpStatusCode? StatusCode => HttpResponseMessage?.StatusCode;
    }

    public class ApiResponse<T> : ApiResponse where T : class
    {
        public ApiResponse(T value, HttpResponseMessageWrapper httpResponseMessage)
            : base(httpResponseMessage)
        {
            Value = value;
        }

        public ApiResponse(T value, string errorMessage, HttpResponseMessageWrapper httpResponseMessage)
            : base(errorMessage, httpResponseMessage)
        {
            Value = value;
        }

        public ApiResponse(T value, bool success, string errorMessage)
            : base(success, errorMessage)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}