namespace E.S.ApiClientHandler.Utils
{
    public class ApiUrlBuilder
    {
        private string _url;

        public ApiUrlBuilder(string url)
        {
            _url = url;
        }

        public ApiUrlBuilder SetUrl(string url)
        {
            _url = url;

            return this;
        }

        public ApiUrlBuilder AddQueryValue(string key, string value)
        {
            if (!string.IsNullOrEmpty(key)
                && !string.IsNullOrEmpty(value))
            {
                if (_url.Contains("?"))
                    _url += $"&{key}={value}";
                else
                    _url += $"?{key}={value}";
            }

            return this;
        }

        public override string ToString()
        {
            return _url;
        }

        public static ApiUrlBuilder New(string url)
        {
            return new ApiUrlBuilder(url);
        }
    }
}