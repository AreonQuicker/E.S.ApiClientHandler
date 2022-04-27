using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace E.S.ApiClientHandler.Utils
{
    public class HttpRequestMessageWrapper
    {
        #region Private Readonly Fields

        private string _baseUrl;

        #endregion

        #region Constructor

        public HttpRequestMessageWrapper(string baseUrl = null)
        {
            HttpRequestMessage = new HttpRequestMessage();

            if (baseUrl != null) WithBaseUrl(baseUrl);
        }

        #endregion

        #region Properties

        public HttpRequestMessage HttpRequestMessage { get; }

        #endregion

        #region Methods

        public HttpRequestMessageWrapper AddKeyAndHeaders(Dictionary<string, string> keyAndHeaders)
        {
            if (keyAndHeaders != null)
                foreach (var keyAndHeader in keyAndHeaders)
                {
                    if (string.IsNullOrEmpty(keyAndHeader.Value)) continue;

                    HttpRequestMessage.Headers.Add(keyAndHeader.Key, keyAndHeader.Value);
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
            HttpRequestMessage.Headers.Add(key, value);

            return this;
        }

        public HttpRequestMessageWrapper WithMethod(HttpMethod httpMethod)
        {
            HttpRequestMessage.Method = httpMethod;

            return this;
        }

        public HttpRequestMessageWrapper WithUrl(string url)
        {
            if (_baseUrl != null)
                HttpRequestMessage.RequestUri = new Uri(_baseUrl + url);
            else
                HttpRequestMessage.RequestUri = new Uri(url);

            return this;
        }

        public HttpRequestMessageWrapper WithUrl(ApiUrlBuilder requestUrlBuilder)
        {
            if (_baseUrl != null)
                HttpRequestMessage.RequestUri
                    = new Uri(_baseUrl + requestUrlBuilder);
            else
                HttpRequestMessage.RequestUri
                    = new Uri(requestUrlBuilder.ToString());

            return this;
        }

        public HttpRequestMessageWrapper WithContent(object data)
        {
            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data);

                HttpRequestMessage.Content =
                    new StringContent(jsonData, Encoding.UTF8, "application/json");
            }

            return this;
        }

        #endregion
    }
}