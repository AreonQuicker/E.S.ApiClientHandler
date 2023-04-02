using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using E.S.ApiClientHandler.Extensions;

namespace E.S.ApiClientHandler.Utils
{
    public class HttpResponseMessageWrapper
    {
        #region Constructor

        public HttpResponseMessageWrapper(HttpResponseMessage httpResponseMessage)
        {
            HttpResponseMessage = httpResponseMessage;
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        public HttpResponseMessage HttpResponseMessage { get; }

        public bool IsSuccess => HttpResponseMessage.IsSuccessStatusCode;

        public HttpStatusCode StatusCode => HttpResponseMessage.StatusCode;

        #endregion

        #region Methods

        public Task<T> ResponseAsAsync<T>(T defaultValue = null, bool shouldBeSuccess = true) where T : class, new()
        {
            return HttpResponseMessage.ToAsync(defaultValue, shouldBeSuccess);
        }

        public Task<byte[]> ResponseAsBytesAsync()
        {
            return HttpResponseMessage.ToBytesAsync();
        }

        #endregion
    }
}