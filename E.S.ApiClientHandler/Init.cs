using System;
using System.Net.Http;
using E.S.ApiClientHandler.Constants;
using E.S.ApiClientHandler.Handlers;
using E.S.ApiClientHandler.Interfaces;
using E.S.ApiClientHandler.Models;
using E.S.Logging.Enums;
using E.S.Logging.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace E.S.ApiClientHandler;

public static class Init
{
    public static IServiceCollection AddHttpClientHandler<TInterface, TImplementation>(this IServiceCollection services,
        string baseAddress, bool logRequest = true) where TInterface : class where TImplementation : class, TInterface
    {
        return services.AddHttpClientHandler<TInterface, TImplementation, ApiAuthorizationHeaderFake>(baseAddress,
            logRequest);
    }

    public static IServiceCollection AddHttpClientHandler<TInterface, TImplementation, TApiAuthorizationHeader>(
        this IServiceCollection services,
        string baseAddress, bool logRequest = true) where TInterface : class
        where TImplementation : class, TInterface
        where TApiAuthorizationHeader : class, IApiAuthorizationHeader
    {
        services.AddScoped<IApiAuthorizationHeader, TApiAuthorizationHeader>();
        services.AddScoped<HttpApiClientMessageHandler>(s =>
            new HttpApiClientMessageHandler(s.GetRequiredService<IServiceProvider>(),
                s.GetRequiredService<IApiAuthorizationHeader>(), logRequest));

        services.AddHttpClient<TInterface, TImplementation>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = TimeSpan.FromMinutes(4);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(4))
            .AddHttpMessageHandler<HttpApiClientMessageHandler>()
            .AddPolicyHandler((serviceProvider, request) => Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .OrResult(o => !o.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (result, span, retryCount, context) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<TImplementation>>();

                        if (result.Exception is HttpRequestException exception)
                            logger.LogErrorOperationWithExtraFormat(
                                LoggerStatusEnum.Error,
                                ApiLoggerConstant.System,
                                null,
                                request.RequestUri?.ToString(),
                                null,
                                $"Api Http request failed (Retries: {retryCount})",
                                exception,
                                Constants.ApiConstants.LoggerFormat,
                                null,
                                request.RequestUri?.ToString(),
                                request.Method.ToString()
                            );
                        else
                            logger.LogErrorOperationWithExtraFormat(
                                LoggerStatusEnum.Error,
                                ApiLoggerConstant.System,
                                null,
                                request.RequestUri?.ToString(),
                                null,
                                $"Api Http request failed (Retries: {retryCount})",
                                null,
                                Constants.ApiConstants.LoggerFormat,
                                result.Result.StatusCode.ToString(),
                                request.RequestUri?.ToString(),
                                request.Method.ToString()
                            );
                    }))
            .AddPolicyHandler(Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(15)));

        return services;
    }
}