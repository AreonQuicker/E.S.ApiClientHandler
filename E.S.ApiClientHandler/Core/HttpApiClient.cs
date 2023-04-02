using System;
using System.Net.Http;
using E.S.ApiClientHandler.Interfaces;

namespace E.S.ApiClientHandler.Core
{
    public class HttpApiClient : IHttpApiClient
    {
        #region Private Readonly Fields

        private readonly bool _httpClientCreatedInService;

        #endregion

        #region Methods

        public void SetBaseUrl(string baseUrl)
        {
            if (HttpClient != null) HttpClient.BaseAddress = new Uri(baseUrl);
        }

        #endregion

        public void Dispose()
        {
            if (_httpClientCreatedInService && HttpClient != null)
            {
                HttpClient.Dispose();
                HasBeenDisposed = true;
            }
        }

        #region Private Methods

        private HttpClient GetHttpClient(string baseUrl = null,
            string authorizationHeader = null,
            string acceptHeader = null)
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };

            if (!string.IsNullOrEmpty(baseUrl)) httpClient.BaseAddress = new Uri(baseUrl);

            if (!string.IsNullOrEmpty(authorizationHeader))
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            if (!string.IsNullOrEmpty(acceptHeader)) httpClient.DefaultRequestHeaders.Add("Accept", acceptHeader);

            return httpClient;
        }

        #endregion

        #region Fields

        #endregion

        #region Constructors

        public HttpApiClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpApiClient(
            string baseUrl = null,
            string authorizationHeader = null,
            string acceptHeader = null)
        {
            _httpClientCreatedInService = true;
            HttpClient = GetHttpClient(baseUrl, authorizationHeader, acceptHeader);
        }

        #endregion

        #region Properties

        public HttpClient HttpClient { get; }

        public bool HasBeenDisposed { get; private set; }

        #endregion
    }
}