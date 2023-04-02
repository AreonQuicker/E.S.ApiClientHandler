using System;
using System.Net.Http;

namespace E.S.ApiClientHandler.Interfaces
{
    public interface IHttpApiClient : IDisposable
    {
        HttpClient HttpClient { get; }

        bool HasBeenDisposed { get; }
        void SetBaseUrl(string baseUrl);
        new void Dispose();
    }
}