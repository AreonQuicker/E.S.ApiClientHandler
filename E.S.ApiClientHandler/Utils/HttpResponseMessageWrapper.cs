using E.S.ApiClientHandler.Extensions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace E.S.ApiClientHandler.Utils
{
    public class HttpResponseMessageWrapper
    {
        #region Fields
        private readonly HttpResponseMessage _httpResponseMessage;
        #endregion

        #region Constructor
        public HttpResponseMessageWrapper(HttpResponseMessage httpResponseMessage)
        {
            _httpResponseMessage = httpResponseMessage;
        }
        #endregion

        #region Properties
        public HttpResponseMessage HttpResponseMessage => _httpResponseMessage;

        public bool IsSuccess => _httpResponseMessage.IsSuccessStatusCode;

        public HttpStatusCode StatusCode => _httpResponseMessage.StatusCode;
        #endregion

        #region Methods
        public Task<T> ResponseAsAsync<T>(T defaultValue = null, bool shouldBeSuccess = true) where T : class, new ()
        {
            return _httpResponseMessage.ToAsync<T>(defaultValue, shouldBeSuccess);
        }

        public Task<byte[]> ResponseAsBytesAsync()
        {
            return _httpResponseMessage.ToBytesAsync();
        }

        #endregion
    }
}
