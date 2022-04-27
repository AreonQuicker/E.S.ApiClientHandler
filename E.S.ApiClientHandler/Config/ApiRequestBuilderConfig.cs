using E.S.ApiClientHandler.Interfaces;

namespace E.S.ApiClientHandler.Config
{
    public class ApiRequestBuilderConfig
    {
        public ApiRequestBuilderConfig(bool throwApiException, ApiRequestBuilderLoggerConfig logger,
            IApiCachingManager apiCachingManager, int? absoluteExpirationRelativeToNowInSeconds)
        {
            ThrowApiException = throwApiException;
            Logger = logger;
            AbsoluteExpirationRelativeToNowInSeconds = absoluteExpirationRelativeToNowInSeconds;
            ApiCachingManager = apiCachingManager;
        }

        public bool ThrowApiException { get; } = true;

        public ApiRequestBuilderLoggerConfig Logger { get; }

        public int? AbsoluteExpirationRelativeToNowInSeconds { get; private set; }

        public IApiCachingManager ApiCachingManager { get; private set; }

        public bool UseLogger => Logger != null && Logger.Logger != null;

        public bool UseCache => ApiCachingManager != null && AbsoluteExpirationRelativeToNowInSeconds.HasValue;

        public void SetCache(IApiCachingManager apiCachingManager,
            int absoluteExpirationRelativeToNowInSeconds)
        {
            ApiCachingManager = apiCachingManager;
            AbsoluteExpirationRelativeToNowInSeconds = absoluteExpirationRelativeToNowInSeconds;
        }

        public static ApiRequestBuilderConfig Create(bool throwApiException, ApiRequestBuilderLoggerConfig useLogger)
        {
            return new ApiRequestBuilderConfig(throwApiException, useLogger, null, null);
        }

        public static ApiRequestBuilderConfig Create(bool throwApiException, ApiRequestBuilderLoggerConfig useLogger,
            IApiCachingManager apiCachingManager, int? absoluteExpirationRelativeToNowInSeconds)
        {
            return new ApiRequestBuilderConfig(throwApiException, useLogger, apiCachingManager,
                absoluteExpirationRelativeToNowInSeconds
            );
        }

        public static ApiRequestBuilderConfig Create(bool throwApiException)
        {
            return new ApiRequestBuilderConfig(throwApiException, null, null, null);
        }

        public static ApiRequestBuilderConfig Create()
        {
            return new ApiRequestBuilderConfig(true, null, null, null);
        }
    }
}