using E.S.ApiClientHandler.Interfaces;
using Microsoft.Extensions.Logging;

namespace E.S.ApiClientHandler.Core
{
    public class ApiIntegration : ApiIntegrationBase, IApiIntegration
    {
        public ApiIntegration(string apiBaseUrl,
            string authorizationHeader = null,
            string userName = null,
            ILogger logger = null,
            IApiCachingManager cachingManager = null,
            int absoluteExpirationRelativeToNowInSeconds = 600) : base(apiBaseUrl,
            authorizationHeader,
            userName,
            logger,
            cachingManager,
            absoluteExpirationRelativeToNowInSeconds)
        {
        }

        public ApiIntegration(string apiBaseUrl) : base(apiBaseUrl)
        {
        }

        public ApiIntegration(string apiBaseUrl,
            string authorizationHeader = null,
            string userName = null) : base(apiBaseUrl,
            authorizationHeader,
            userName)
        {
        }

        public ApiIntegration(string apiBaseUrl,
            string authorizationHeader = null,
            string userName = null,
            ILogger logger = null) : base(apiBaseUrl,
            authorizationHeader,
            userName,
            logger)
        {
        }
    }
}