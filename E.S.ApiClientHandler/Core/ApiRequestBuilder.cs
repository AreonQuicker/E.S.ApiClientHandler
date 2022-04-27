using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using E.S.ApiClientHandler.Config;
using E.S.ApiClientHandler.Exceptions;
using E.S.ApiClientHandler.Extensions;
using E.S.ApiClientHandler.Interfaces;
using E.S.ApiClientHandler.Models;
using E.S.ApiClientHandler.Utils;
using E.S.Logging.Enums;
using E.S.Logging.Extensions;

namespace E.S.ApiClientHandler.Core
{
    public class ApiRequestBuilder : IApiRequestBuilder, IApiRequestBuilderInner1, IApiRequestBuilderInner2
    {
        #region IDisposable

        public void Dispose()
        {
            DisposeHttpClient();
            _hasBeenDisposed = true;
        }

        #endregion

        #region Private Fields

        private readonly IHttpApiClient _httpApiClient;
        private readonly string _baseUrl;
        private readonly ApiRequestBuilderConfig _apiRequestBuilderConfig;
        private bool _hasBeenDisposed;
        private HttpRequestMessageWrapper _httpRequestMessageWrapper;

        #endregion

        #region Constructor

        private ApiRequestBuilder(string baseUrl, ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            _baseUrl = baseUrl;
            _apiRequestBuilderConfig = apiRequestBuilderConfig;
            if (_apiRequestBuilderConfig == null) _apiRequestBuilderConfig = ApiRequestBuilderConfig.Create();
        }

        public ApiRequestBuilder(string baseUrl,
            string authorizationHeader = null,
            string acceptHeader = null,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
            : this(baseUrl, apiRequestBuilderConfig)
        {
            _httpApiClient = new HttpApiClient(baseUrl, authorizationHeader, acceptHeader);
            _httpRequestMessageWrapper = new HttpRequestMessageWrapper(baseUrl);
            if (!string.IsNullOrEmpty(authorizationHeader))
                _httpRequestMessageWrapper.AddHeader("Authorization", authorizationHeader);
        }

        public ApiRequestBuilder(
            HttpClient httpClient,
            string baseUrl,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
            : this(baseUrl, apiRequestBuilderConfig)
        {
            _httpApiClient = new HttpApiClient(httpClient);
            _httpRequestMessageWrapper = new HttpRequestMessageWrapper(baseUrl);
        }

        public ApiRequestBuilder(
            HttpClient httpClient,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
            : this(httpClient.BaseAddress?.ToString(), apiRequestBuilderConfig)
        {
            _httpApiClient = new HttpApiClient(httpClient);
            _httpRequestMessageWrapper = new HttpRequestMessageWrapper(httpClient.BaseAddress?.ToString());
        }

        public ApiRequestBuilder(
            IHttpApiClient httpApiClient,
            string baseUrl,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
            : this(baseUrl, apiRequestBuilderConfig)
        {
            _httpApiClient = httpApiClient;
            _httpRequestMessageWrapper = new HttpRequestMessageWrapper(baseUrl);
        }

        public ApiRequestBuilder(
            IHttpApiClient httpApiClient,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
            : this(httpApiClient.HttpClient.BaseAddress?.ToString(), apiRequestBuilderConfig)
        {
            _httpApiClient = httpApiClient;
            _httpRequestMessageWrapper =
                new HttpRequestMessageWrapper(httpApiClient.HttpClient.BaseAddress?.ToString());
        }

        #endregion

        #region IAPIClient

        public IApiRequestBuilderInner1 New()
        {
            return new ApiRequestBuilder(_httpApiClient, _baseUrl, _apiRequestBuilderConfig);
        }

        public IApiRequestBuilder SetUser(string user)
        {
            _apiRequestBuilderConfig?.Logger?.SetUser(user);

            return this;
        }

        public IApiRequestBuilder WithCacheClient(IApiCachingManager apiCachingManager,
            int absoluteExpirationRelativeToNowInSeconds)
        {
            _apiRequestBuilderConfig.SetCache(apiCachingManager, absoluteExpirationRelativeToNowInSeconds);

            return this;
        }

        public IApiRequestBuilderInner1 WithHttpRequestMessage(HttpRequestMessageWrapper httpRequestMessageWrapper)
        {
            _httpRequestMessageWrapper = httpRequestMessageWrapper;

            return this;
        }

        public IApiRequestBuilderInner1 AddKeyAndHeaders(Dictionary<string, string> keyAndHeaders)
        {
            _httpRequestMessageWrapper.AddKeyAndHeaders(keyAndHeaders);

            return this;
        }

        public IApiRequestBuilderInner1 AddHeader(string key, string header)
        {
            _httpRequestMessageWrapper.AddHeader(key, header);

            return this;
        }

        public IApiRequestBuilderInner1 WithContent(object content)
        {
            _httpRequestMessageWrapper.WithContent(content);

            return this;
        }

        public IApiRequestBuilderInner1 WithMethod(HttpMethod httpMethod)
        {
            _httpRequestMessageWrapper.WithMethod(httpMethod);

            return this;
        }

        public IApiRequestBuilderInner2 WithUrl(ApiUrlBuilder requestUrlBuilder)
        {
            _httpRequestMessageWrapper.WithUrl(requestUrlBuilder);

            return this;
        }

        public IApiRequestBuilderInner2 WithUrl(string url)
        {
            _httpRequestMessageWrapper.WithUrl(url);

            return this;
        }

        public async Task<ApiResponse<T>> ExecuteAsync<T>(T defaultValue = null) where T : class, new()
        {
            if (_hasBeenDisposed) throw new ApiException("Service has already been disposed");

            if (_httpApiClient.HasBeenDisposed) throw new ApiException("Http Client has already been disposed");

            try
            {
                var cache = GetCache();

                if (cache.useCache)
                {
                    var cacheResult = await _apiRequestBuilderConfig.ApiCachingManager.GetAsync<T>(cache.cacheKey);

                    if (cacheResult != null) return new ApiResponse<T>(cacheResult, true, null);
                }

                var response = await SendAsync(_httpRequestMessageWrapper).ConfigureAwait(false);

                var errorHandler = await HandleResponseMessageWrapperErrorAsync<T>(response);
                if (!errorHandler.Handled)
                    await HandleResponseMessageErrorAsync(response.HttpResponseMessage);

                var errorHandler3 = HandleRequestError(response);

                if (_apiRequestBuilderConfig.ThrowApiException)
                {
                    if (errorHandler.Handled)
                        throw errorHandler.Error;

                    if (errorHandler3.Handled)
                        throw errorHandler3.Error;
                }

                if (response.IsSuccess)
                {
                    var value = await response.ResponseAsAsync(defaultValue);

                    if (cache.useCache
                        && value != null)
                        await _apiRequestBuilderConfig.ApiCachingManager.SetAsync(cache.cacheKey, value,
                            _apiRequestBuilderConfig.AbsoluteExpirationRelativeToNowInSeconds);

                    return new ApiResponse<T>(value, response);
                }
                else
                {
                    var value = await response.ResponseAsAsync(defaultValue, false);

                    return new ApiResponse<T>(value ?? defaultValue ?? default(T), response);
                }
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);

                throw;
            }
            finally
            {
                DisposeHttpClient();
            }
        }

        public async Task<ApiResponse<string>> ExecuteAsync()
        {
            if (_hasBeenDisposed) throw new ApiException("Service has already been disposed");

            if (_httpApiClient.HasBeenDisposed) throw new ApiException("Http Client has already been disposed");

            try
            {
                var response = await SendAsync(_httpRequestMessageWrapper).ConfigureAwait(false);
                await HandleResponseMessageErrorAsync(response.HttpResponseMessage);

                var errorHandler3 = HandleRequestError(response);

                if (_apiRequestBuilderConfig.ThrowApiException)
                    if (errorHandler3.Handled)
                        throw errorHandler3.Error;

                var value = await response.HttpResponseMessage.ToStringAsync(false);

                if (response.IsSuccess)
                    return new ApiResponse<string>(value, response);
                else
                    return new ApiResponse<string>(value, response);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);

                throw;
            }
            finally
            {
                DisposeHttpClient();
            }
        }

        public async Task<ApiResponse> ExecuteNoValueAsync()
        {
            if (_hasBeenDisposed) throw new ApiException("Service has already been disposed");

            if (_httpApiClient.HasBeenDisposed) throw new ApiException("Http Client has already been disposed");

            try
            {
                var response = await SendAsync(_httpRequestMessageWrapper).ConfigureAwait(false);

                await HandleResponseMessageErrorAsync(response.HttpResponseMessage);
                var errorHandler3 = HandleRequestError(response);

                if (_apiRequestBuilderConfig.ThrowApiException)
                    if (errorHandler3.Handled)
                        throw errorHandler3.Error;

                if (response.IsSuccess) return new ApiResponse(response);

                return new ApiResponse(response);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);

                throw;
            }
            finally
            {
                DisposeHttpClient();
            }
        }

        #endregion

        #region Static Methods

        public static IApiRequestBuilder Make(string baseUrl,
            string authorizationHeader = null,
            string acceptHeader = null,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            return new ApiRequestBuilder(baseUrl, authorizationHeader, acceptHeader, apiRequestBuilderConfig);
        }

        public static IApiRequestBuilder Make(string baseUrl,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            return new ApiRequestBuilder(baseUrl, null, null, apiRequestBuilderConfig);
        }

        public static IApiRequestBuilder Make(
            HttpClient httpClient,
            string baseUrl,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            return new ApiRequestBuilder(httpClient, baseUrl, apiRequestBuilderConfig);
        }

        public static IApiRequestBuilder Make(
            HttpClient httpClient,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            return new ApiRequestBuilder(httpClient, apiRequestBuilderConfig);
        }

        #endregion

        #region Private Methods

        private void LogException(ApiException apx)
        {
            if (_apiRequestBuilderConfig.UseLogger)
                _apiRequestBuilderConfig.Logger.Logger.LogErrorOperationWithExtraFormat(
                    LoggerStatusEnum.Error,
                    "Api Client",
                    "Api Call",
                    apx?.StatusCode.ToString(),
                    _apiRequestBuilderConfig.Logger?.User,
                    apx?.Message,
                    apx,
                    _apiRequestBuilderConfig.Logger.Format,
                    apx?.StatusCode.ToString(),
                    apx?.Url
                );
        }

        private void LogException(Exception apx)
        {
            if (_apiRequestBuilderConfig.UseLogger)
                _apiRequestBuilderConfig.Logger.Logger.LogErrorOperationWithExtraFormat(
                    LoggerStatusEnum.Error,
                    "Api Client",
                    "Api Call",
                    "",
                    _apiRequestBuilderConfig.Logger?.User,
                    apx?.Message,
                    apx,
                    _apiRequestBuilderConfig.Logger.Format,
                    "",
                    _httpRequestMessageWrapper?.HttpRequestMessage?.RequestUri.ToString()
                );
        }

        private async Task<(bool Handled, ApiException Error)> HandleResponseMessageErrorAsync(
            HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var contentResponse = await response.ToStringAsync(false);
                var firstLine = contentResponse
                    .Split(new[] {Environment.NewLine, "\n\n"}, StringSplitOptions.None)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(firstLine))
                {
                    var error = new ApiException(
                        firstLine,
                        response.StatusCode,
                        response?.RequestMessage?.RequestUri.ToString());

                    LogException(error);

                    return (true, error);
                }
            }

            return (false, null);
        }

        private async Task<(bool Handled, ApiException Error)> HandleResponseMessageWrapperErrorAsync<T>(
            HttpResponseMessageWrapper response) where T : class, new()
        {
            if (!response.IsSuccess
                && typeof(T).IsGenericType
                && typeof(T).GetGenericTypeDefinition() == typeof(ContentResponse<>))
            {
                var contentResponse = await response.ResponseAsAsync<T>(null, false);

                if (contentResponse is ContentResponse t && !string.IsNullOrEmpty(t.Message))
                {
                    var error = new ApiException(
                        t.Message,
                        response.StatusCode,
                        response?.HttpResponseMessage?.RequestMessage?.RequestUri.ToString(),
                        t.Exception);

                    LogException(error);

                    return (true, error);
                }
            }

            return (false, null);
        }

        private (bool Handled, ApiException Error) HandleRequestError(HttpResponseMessageWrapper response)
        {
            try
            {
                response.HttpResponseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                var error = new ApiException(
                    ex.Message,
                    response.StatusCode,
                    response?.HttpResponseMessage?.RequestMessage?.RequestUri.ToString(),
                    ex);

                LogException(error);

                return (true, error);
            }

            return (false, null);
        }

        private (bool useCache, string cacheKey) GetCache()
        {
            var useCache = _apiRequestBuilderConfig.UseCache
                           && _httpRequestMessageWrapper.HttpRequestMessage.Method == HttpMethod.Get;

            if (!useCache) return (false, null);

            var cacheKey = $"Api/{_httpRequestMessageWrapper.HttpRequestMessage.RequestUri}";

            if (_httpRequestMessageWrapper.HttpRequestMessage.Headers != null)
                foreach (var header in _httpRequestMessageWrapper.HttpRequestMessage.Headers)
                {
                    if (header.Key.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase)
                        || header.Key.Equals("Accept", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    cacheKey += $"/Key={header.Key}Value={header.Value}";
                }

            return (true, cacheKey);
        }

        private void DisposeHttpClient()
        {
            _httpApiClient.Dispose();
        }

        private async Task<HttpResponseMessageWrapper> SendAsync(HttpRequestMessageWrapper apiRequest)
        {
            var response = await _httpApiClient.HttpClient.SendAsync(apiRequest.HttpRequestMessage)
                .ConfigureAwait(false);

            return new HttpResponseMessageWrapper(response);
        }

        #endregion
    }
}