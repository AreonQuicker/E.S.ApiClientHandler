using E.S.ApiClientHandler.Interfaces;

namespace E.S.ApiClientHandler.Config
{
    public class ApiRequestBuilderConfig
    {
        private ApiRequestBuilderConfig(IApiCachingManager apiCachingManager,
            int? absoluteExpirationRelativeToNowInSeconds)
        {
            AbsoluteExpirationRelativeToNowInSeconds = absoluteExpirationRelativeToNowInSeconds;
            ApiCachingManager = apiCachingManager;
        }

        public int? AbsoluteExpirationRelativeToNowInSeconds { get; private set; }

        public IApiCachingManager ApiCachingManager { get; private set; }
        public bool UseCache => ApiCachingManager != null && AbsoluteExpirationRelativeToNowInSeconds.HasValue;

        public void SetCache(IApiCachingManager apiCachingManager,
            int absoluteExpirationRelativeToNowInSeconds)
        {
            ApiCachingManager = apiCachingManager;
            AbsoluteExpirationRelativeToNowInSeconds = absoluteExpirationRelativeToNowInSeconds;
        }

        public static ApiRequestBuilderConfig Create()
        {
            return new ApiRequestBuilderConfig(null,
                null
            );
        }

        public static ApiRequestBuilderConfig Create(IApiCachingManager apiCachingManager,
            int? absoluteExpirationRelativeToNowInSeconds)
        {
            return new ApiRequestBuilderConfig(apiCachingManager,
                absoluteExpirationRelativeToNowInSeconds
            );
        }
    }
}