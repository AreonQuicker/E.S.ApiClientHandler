using System.Net.Http;
using E.S.ApiClientHandler.Interfaces;
using Microsoft.Extensions.Logging;

namespace E.S.ApiClientHandler.Core
{
    public class ApiIntegration : ApiIntegrationBase
    {
        public ApiIntegration(HttpClient client) : base(client)
        {
        }

        public ApiIntegration(HttpClient client, IApiCachingManager cachingManager,
            int absoluteExpirationRelativeToNowInSeconds) : base(client, cachingManager,
            absoluteExpirationRelativeToNowInSeconds)
        {
        }

        public ApiIntegration(string apiBaseUrl, string authorizationHeader) : base(apiBaseUrl, authorizationHeader)
        {
        }

        public ApiIntegration(string apiBaseUrl, string authorizationHeader, IApiCachingManager cachingManager,
            int absoluteExpirationRelativeToNowInSeconds) : base(apiBaseUrl, authorizationHeader, cachingManager,
            absoluteExpirationRelativeToNowInSeconds)
        {
        }
    }
}