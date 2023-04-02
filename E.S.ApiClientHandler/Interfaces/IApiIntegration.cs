using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace E.S.ApiClientHandler.Interfaces
{
    public interface IApiIntegration : IDisposable
    {
        HttpClient Client { get; }
        IApiRequestBuilder ApiRequestBuilder { get; }
        Task<T> GetAsync<T>(string path, bool withCache = false) where T : class, new();
        T Get<T>(string path, bool withCache = false) where T : class, new();
        Task<T> GetAsIsAsync<T>(string path, bool withCache = false) where T : class, new();

        Task<string> GetAsIsAsync(string path, bool withCache = false);
        T GetAsIs<T>(string path, bool withCache = false) where T : class, new();
        Task<List<T>> GetListAsync<T>(string path, bool withCache = false) where T : class;
        List<T> GetList<T>(string path, bool withCache = false) where T : class;
        Task<List<T>> GetAsIsListAsync<T>(string path, bool withCache = false) where T : class;
        List<T> GetAsIsList<T>(string path, bool withCache = false) where T : class;
        Task<T> PostAsync<T>(string path, object content) where T : class, new();
        T Post<T>(string path, object content) where T : class, new();
        Task<T> PostAsIsAsync<T>(string path, object content) where T : class, new();
        Task<string> PostAsIsAsync(string path, object content);
        T PostAsIs<T>(string path, object content) where T : class, new();
        Task<List<T>> PostListAsync<T>(string path, object content) where T : class;
        List<T> PostList<T>(string path, object content) where T : class;
    }
}