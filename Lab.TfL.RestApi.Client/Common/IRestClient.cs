using Lab.TfL.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.TfL.RestApi.Client.Common
{
    public interface IRestClient
    {
        Task<ApiResponse<T>> GetAsync<T>(Uri requestUrl, string namedClient, string bearerToken = null, IDictionary<string, string> headers = null);

        Task<ApiResponse<T>> PostAsync<T>(Uri requestUrl, string namedClient, object jsonObject, string bearerToken = null, IDictionary<string, string> headers = null);
    }
}