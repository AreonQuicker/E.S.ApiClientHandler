using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using E.S.ApiClientHandler.Constants;
using E.S.ApiClientHandler.Interfaces;
using E.S.Logging.Enums;
using E.S.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace E.S.ApiClientHandler.Handlers;

public class HttpApiClientMessageHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApiAuthorizationHeader _authorizationHeader;
    private readonly bool _logRequest;

    public HttpApiClientMessageHandler(IServiceProvider serviceProvider,
        IApiAuthorizationHeader authorizationHeader, bool logRequest)
    {
        _serviceProvider = serviceProvider;
        _authorizationHeader = authorizationHeader;
        _logRequest = logRequest;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authorizationHeader = _authorizationHeader.GetAuthorizationHeader();
        if (!string.IsNullOrEmpty(authorizationHeader))
            request.Headers.Add("Authorization", authorizationHeader);

        using (var scope = _serviceProvider.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<HttpApiClientMessageHandler>>();

            using (logger.BeginScope("Handling Api Http request"))
            {
                Stopwatch timer = null;
                
                if (_logRequest)
                {
                    timer = new Stopwatch();
                    timer.Start();
                }

                var response = await base.SendAsync(request, cancellationToken);

                timer?.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogErrorOperationWithExtraFormat(
                        LoggerStatusEnum.Error,
                        ApiLoggerConstant.System,
                        null,
                        request.RequestUri?.ToString(),
                        null,
                        "Api Http request failed",
                        null,
                        Constants.ApiConstants.LoggerFormat,
                        response.StatusCode.ToString(),
                        request.RequestUri?.ToString(),
                        request.Method.ToString()
                    );
                }
                else
                {
                    if (timer != null)
                    {
                        var timeTakenString = "Time taken: " + timer.Elapsed.ToString(@"m\:ss\.fff");

                        logger.LogOperationWithExtraFormat(
                            LogLevel.Information,
                            LoggerStatusEnum.InProgress,
                            ApiLoggerConstant.System,
                            null,
                            request.RequestUri?.ToString(),
                            null,
                            $"Api Http request:: ({timeTakenString})",
                            null,
                            Constants.ApiConstants.LoggerFormat,
                            null,
                            request.RequestUri?.ToString(),
                            request.Method.ToString()
                        );
                    }
                }

                return response;
            }
        }
    }
}