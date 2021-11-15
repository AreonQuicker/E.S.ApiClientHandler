using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace E.S.ApiClientHandler.Utils
{
    public class HttpRequestMessageWrapper
    {
        #region Private Readonly Fields
        private string _baseUrl;

        private readonly HttpRequestMessage _httpRequestMessage;
        #endregion

        #region Constructor
        public HttpRequestMessageWrapper(string baseUrl = null)
        {
            _httpRequestMessage = new HttpRequestMessage();

            if (baseUrl != null)
            {
                WithBaseUrl(baseUrl);
            }
        }
        #endregion

        #region Properties
        public HttpRequestMessage HttpRequestMessage => _httpRequestMessage;
        #endregion

        #region Methods
        public HttpRequestMessageWrapper AddKeyAndHeaders(Dictionary<string, string> keyAndHeaders)
        {
            if (keyAndHeaders != null)
            {
                foreach (KeyValuePair<string, string> keyAndHeader in keyAndHeaders)
                {
                    if (string.IsNullOrEmpty(keyAndHeader.Value))
                    {
                        continue;
                    }

                    _httpRequestMessage.Headers.Add(keyAndHeader.Key, keyAndHeader.Value);
                }
            }

            return this;
        }

        public HttpRequestMessageWrapper WithBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;

            return this;
        }

        public HttpRequestMessageWrapper AddHeader(string key, string value)
        {
            _httpRequestMessage.Headers.Add(key, value);

            return this;
        }

        public HttpRequestMessageWrapper WithMethod(HttpMethod httpMethod)
        {
            _httpRequestMessage.Method = httpMethod;

            return this;
        }

        public HttpRequestMessageWrapper WithUrl(string url)
        {
            if (_baseUrl != null)
            {
                _httpRequestMessage.RequestUri = new Uri(_baseUrl + url);
            }
            else
            {
                _httpRequestMessage.RequestUri = new Uri(url);
            }

            return this;
        }

        public HttpRequestMessageWrapper WithUrl(ApiUrlBuilder requestUrlBuilder)
        {
            if (_baseUrl != null)
            {
                _httpRequestMessage.RequestUri
                    = new Uri(_baseUrl + requestUrlBuilder.ToString());
            }
            else
            {
                _httpRequestMessage.RequestUri
                    = new Uri(requestUrlBuilder.ToString());
            }

            return this;
        }

        public HttpRequestMessageWrapper WithContent(object data)
        {
            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data);

                _httpRequestMessage.Content =
                     new StringContent(jsonData, Encoding.UTF8, "application/json");
            }

            return this;
        }
        #endregion
    }
}
