namespace E.S.ApiClientHandler.Config
{
    public class ApiRequestBuilderConfig
    {
        public bool ThrowApiException { get; } = true;

        public bool EmptyListIfNull { get; set; } = true;

        public ApiRequestBuilderLoggerConfig Logger { get; } = null;

        public ApiRequestBuilderConfig(bool throwApiException, ApiRequestBuilderLoggerConfig logger)
        {
            ThrowApiException = throwApiException;
            Logger = logger;   
        }

        public bool UseLogger => Logger != null && Logger.Logger != null;

        public static ApiRequestBuilderConfig Create(bool throwApiException, ApiRequestBuilderLoggerConfig useLogger)
        {
            return new ApiRequestBuilderConfig(throwApiException, useLogger);
        }

        public static ApiRequestBuilderConfig Create(bool throwApiException)
        {
            return new ApiRequestBuilderConfig(throwApiException, null);
        }

        public static ApiRequestBuilderConfig Create()
        {
            return new ApiRequestBuilderConfig(true, null);
        }
    }
}
