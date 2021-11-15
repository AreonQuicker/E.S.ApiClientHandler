using E.S.ApiClientHandler.Interfaces;
using System;
using System.Net.Http;

namespace E.S.ApiClientHandler.Core
{
    public class HttpApiClient : IHttpApiClient
    {
        #region Private Readonly Fields
        private readonly HttpClient _httpClient;
        private readonly bool _httpClientCreatedInService = false;
        private bool _hasBeenDisposed;
        #endregion

        #region Fields        
        #endregion

        #region Constructors
        public HttpApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpApiClient(
           string baseUrl = null,
           string authorizationHeader = null,
           string acceptHeader = null)
        {
            _httpClientCreatedInService = true;
            _httpClient = GetHttpClient(baseUrl, authorizationHeader, acceptHeader);
        }

        #endregion

        #region Methods

        public void SetBaseUrl(string baseUrl)
        {
            if (_httpClient != null)
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
        }

        #endregion

        #region Private Methods

        private HttpClient GetHttpClient(string baseUrl = null,
            string authorizationHeader = null,
            string acceptHeader = null)
        {
            HttpClient httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(10)
            };

            if (!string.IsNullOrEmpty(baseUrl))
            {
                httpClient.BaseAddress = new Uri(baseUrl);
            }

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
            }

            if (!string.IsNullOrEmpty(acceptHeader))
            {
                httpClient.DefaultRequestHeaders.Add("Accept", acceptHeader);
            }

            return httpClient;
        }

        #endregion

        #region Properties
        public HttpClient HttpClient => _httpClient;

        public bool HasBeenDisposed => _hasBeenDisposed;
        #endregion

        public void Dispose()
        {
            if (_httpClientCreatedInService && _httpClient != null)
            {
                _httpClient.Dispose();
                _hasBeenDisposed = true;
            }
        }
    }
}
