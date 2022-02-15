using System;

namespace E.S.ApiClientHandler.Models
{
    public class ContentResponse
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public class ContentResponse<T> : ContentResponse where T : class
    {
        public T Data { get; set; }
    }
}