using Microsoft.Extensions.Logging;

namespace E.S.ApiClientHandler.Config
{
    public class ApiRequestBuilderLoggerConfig
    {
        public ApiRequestBuilderLoggerConfig(ILogger logger, string user = null, string format = null)
        {
            if (format != null) Format = format;

            Logger = logger;
            User = user;
        }

        public ILogger Logger { get; }

        public string User { get; }

        public string Format { get; } =
            "StatusCode:{statusCode} Url:{url}";

        public static ApiRequestBuilderLoggerConfig Create(ILogger logger, string user, string format)
        {
            return new ApiRequestBuilderLoggerConfig(logger, user, format);
        }

        public static ApiRequestBuilderLoggerConfig Create(ILogger logger, string user)
        {
            return new ApiRequestBuilderLoggerConfig(logger, user);
        }

        public static ApiRequestBuilderLoggerConfig Create(ILogger logger)
        {
            return new ApiRequestBuilderLoggerConfig(logger);
        }
    }
}