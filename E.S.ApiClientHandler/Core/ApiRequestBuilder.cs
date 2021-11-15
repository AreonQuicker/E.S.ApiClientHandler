using E.S.ApiClientHandler.Config;
using E.S.ApiClientHandler.Exceptions;
using E.S.ApiClientHandler.Extensions;
using E.S.ApiClientHandler.Interfaces;
using E.S.ApiClientHandler.Models;
using E.S.ApiClientHandler.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace E.S.ApiClientHandler.Core
{

    public class ApiRequestBuilder : IApiRequestBuilder
    {
        #region Private Fields
        private readonly IHttpApiClient _httpApiClient;
        private readonly string _baseUrl;
        private readonly ApiRequestBuilderConfig _apiRequestBuilderConfig;
        private bool _hasBeenDisposed;
        #endregion

        #region Constructor

        #region Fields
        private int? _absoluteExpirationRelativeToNowInSeconds;
        private bool _withCache = false;
        private HttpRequestMessageWrapper _httpRequestMessageWrapper;
        private IApiCachingManager _apiCachingManager;
        #endregion
        public ApiRequestBuilder(string baseUrl, ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            _baseUrl = baseUrl;
            _apiRequestBuilderConfig = apiRequestBuilderConfig;
            if (_apiRequestBuilderConfig == null)
            {
                _apiRequestBuilderConfig = ApiRequestBuilderConfig.Create();
            }
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
            {
                _httpRequestMessageWrapper.AddHeader("Authorization", authorizationHeader);
            }
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
          IHttpApiClient httpApiClient,
          string baseUrl,
          ApiRequestBuilderConfig apiRequestBuilderConfig = null)
          : this(baseUrl, apiRequestBuilderConfig)
        {
            _httpApiClient = httpApiClient;
            _httpRequestMessageWrapper = new HttpRequestMessageWrapper(baseUrl);
        }
        #endregion      

        #region IAPIClient
        public IApiRequestBuilder WithHttpRequestMessage(HttpRequestMessageWrapper httpRequestMessageWrapper)
        {
            _httpRequestMessageWrapper = httpRequestMessageWrapper;

            return this;
        }

        public IApiRequestBuilder WithUrl(string url)
        {
            _httpRequestMessageWrapper.WithUrl(url);

            return this;
        }

        public IApiRequestBuilder WithUrl(ApiUrlBuilder requestUrlBuilder)
        {
            _httpRequestMessageWrapper.WithUrl(requestUrlBuilder);

            return this;
        }

        public IApiRequestBuilder WithMethod(HttpMethod httpMethod)
        {
            _httpRequestMessageWrapper.WithMethod(httpMethod);

            return this;
        }

        public IApiRequestBuilder WithContent(object content)
        {
            _httpRequestMessageWrapper.WithContent(content);

            return this;
        }

        public IApiRequestBuilder AddKeyAndHeaders(Dictionary<string, string> keyAndHeaders)
        {
            _httpRequestMessageWrapper.AddKeyAndHeaders(keyAndHeaders);

            return this;
        }

        public IApiRequestBuilder AddHeader(string key, string header)
        {
            _httpRequestMessageWrapper.AddHeader(key, header);

            return this;
        }

        public IApiRequestBuilder WithCacheClient(IApiCachingManager apiCachingManager, int absoluteExpirationRelativeToNowInSeconds = 300)
        {
            _apiCachingManager = apiCachingManager;
            _absoluteExpirationRelativeToNowInSeconds = absoluteExpirationRelativeToNowInSeconds;
            _withCache = true;

            return this;
        }

        public async Task<ApiResponse<T>> ExecuteAsync<T>(T defaultValue = null) where T : class, new ()
        {
            if (_hasBeenDisposed)
            {
                throw new ApiException("Service has already been disposed");
            }

            if (_httpApiClient.HasBeenDisposed)
            {
                throw new ApiException("Http Client has already been disposed");
            }

            try
            {
                var cache = GetCache();

                if (cache.useCache)
                {
                    T cacheResult = await _apiCachingManager.GetAsync<T>(cache.cacheKey);

                    if (cacheResult != null)
                    {
                        return new ApiResponse<T>(cacheResult, true, null);
                    }
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
                    T value = await response.ResponseAsAsync<T>(defaultValue);

                    if (cache.useCache
                        && value != null)
                    {
                        await _apiCachingManager.SetAsync<T>(cache.cacheKey, value, _absoluteExpirationRelativeToNowInSeconds);
                    }

                    return new ApiResponse<T>(value, response);
                }

                return new ApiResponse<T>(defaultValue ?? default(T), response);
            }
            catch (ApiException ex)
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
            if (_hasBeenDisposed)
            {
                throw new ApiException("Service has already been disposed");
            }

            if (_httpApiClient.HasBeenDisposed)
            {
                throw new ApiException("Http Client has already been disposed");
            }

            try
            {
                HttpResponseMessageWrapper response = await SendAsync(_httpRequestMessageWrapper).ConfigureAwait(false);

                await HandleResponseMessageErrorAsync(response.HttpResponseMessage);
                var errorHandler3 = HandleRequestError(response);

                if (_apiRequestBuilderConfig.ThrowApiException)
                {  
                    if (errorHandler3.Handled)
                        throw errorHandler3.Error;
                }

                if (response.IsSuccess)
                {
                    return new ApiResponse(response);
                }

                return new ApiResponse(response);
            }
            catch (ApiException ex)
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

        public IApiRequestBuilder New()
        {
            return new ApiRequestBuilder(_httpApiClient, _baseUrl, _apiRequestBuilderConfig);
        }

        #endregion

        #region Static Methods

        public static ApiRequestBuilder Make(string baseUrl,
            string authorizationHeader = null,
            string acceptHeader = null,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            return new ApiRequestBuilder(baseUrl, authorizationHeader, acceptHeader, apiRequestBuilderConfig);
        }

        public static ApiRequestBuilder Make(
            HttpClient httpClient,
            string baseUrl,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            return new ApiRequestBuilder(httpClient, baseUrl, apiRequestBuilderConfig);
        }

        public static ApiRequestBuilder Make(
            HttpClient httpClient,
            ApiRequestBuilderConfig apiRequestBuilderConfig = null)
        {
            return new ApiRequestBuilder(httpClient, null, apiRequestBuilderConfig);
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            DisposeHttpClient();
            _hasBeenDisposed = true;
        }
        #endregion

        #region Private Methods     
        private void LogException(ApiException apx)
        {
            if (_apiRequestBuilderConfig.UseLogger)
            {
                _apiRequestBuilderConfig.Logger.Logger.Log(
                    LogLevel.Error,
                    apx,
                    _apiRequestBuilderConfig.Logger.Format,
                    apx.StatusCode,
                    apx.Message,
                    apx.Url ?? "",
                    _apiRequestBuilderConfig.Logger?.User ?? "");
            }
        }

        private void LogException(Exception apx)
        {
            if (_apiRequestBuilderConfig.UseLogger)
            {
                _apiRequestBuilderConfig.Logger.Logger.Log(
                    LogLevel.Error,
                    apx,
                    _apiRequestBuilderConfig.Logger.Format,
                    "",
                    apx.Message,
                    _httpRequestMessageWrapper?.HttpRequestMessage?.RequestUri.ToString() ?? "",
                    _apiRequestBuilderConfig.Logger?.User ?? "");
            }
        }

        private async Task<(bool Handled, ApiException Error)> HandleResponseMessageErrorAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var contentResponse = await response.ToStringAsync(false);
                var firstLine = contentResponse.Split(new string[] { Environment.NewLine, "\n\n" }, StringSplitOptions.None)
                    .FirstOrDefault();

                if (firstLine != null)
                {
                    var error = new ApiException(
                        firstLine,
                        response.StatusCode,
                        response?.RequestMessage?.RequestUri.ToString(),
                        null);

                    LogException(error);

                    return (true, error);
                }
            }

            return (false, null);
        }

        private async Task<(bool Handled, ApiException Error)> HandleResponseMessageWrapperErrorAsync<T>(HttpResponseMessageWrapper response) where T : class,new()
        {
            if (!response.IsSuccess
                && typeof(T).GetGenericTypeDefinition() != null
                && typeof(T).IsGenericType
                && typeof(T).GetGenericTypeDefinition() == typeof(ContentResponse<>))
            {
                var contentResponse = await response.ResponseAsAsync<T>(null, false);

                if (contentResponse is ContentResponse t)
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
            catch (System.Exception ex)
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
            bool useCache = _apiCachingManager != null
                    && _withCache
                    && _httpRequestMessageWrapper.HttpRequestMessage.Method == HttpMethod.Get;

            if (!useCache)
            {
                return (false, null);
            }

            string cacheKey = "Api/" + _httpRequestMessageWrapper.HttpRequestMessage.RequestUri.ToString();

            if (_httpRequestMessageWrapper.HttpRequestMessage.Headers != null)
            {
                foreach (var header in _httpRequestMessageWrapper.HttpRequestMessage.Headers)
                {
                    if (header.Key.Equals("Authorization", System.StringComparison.InvariantCultureIgnoreCase)
                        || header.Key.Equals("Accept", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    cacheKey += "/Key=" + header.Key + "Value=" + header.Value;
                }
            }

            return (true, cacheKey);
        }

        private void DisposeHttpClient()
        {
            _httpApiClient.Dispose();
        }

        private async Task<HttpResponseMessageWrapper> SendAsync(HttpRequestMessageWrapper apiRequest)
        {
            HttpResponseMessage response = await _httpApiClient.HttpClient.SendAsync(apiRequest.HttpRequestMessage).ConfigureAwait(false);

            return new HttpResponseMessageWrapper(response);
        }

        #endregion
    }
}
