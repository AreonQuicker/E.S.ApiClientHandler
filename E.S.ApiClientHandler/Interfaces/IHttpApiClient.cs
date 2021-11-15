using System;
using System.Net.Http;

namespace E.S.ApiClientHandler.Interfaces
{
    public interface IHttpApiClient : IDisposable
    {
        HttpClient HttpClient { get; }
        void SetBaseUrl(string baseUrl);
        new void Dispose();

        bool HasBeenDisposed { get; }
    }
}
