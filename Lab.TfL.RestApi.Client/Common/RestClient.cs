using Lab.TfL.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Lab.TfL.RestApi.Client.Common
{
    public class RestClient : IRestClient
    {
        private readonly ILogger<RestClient> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public RestClient(IHttpClientFactory httpClientFactory, ILogger<RestClient> logger)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<T>> GetAsync<T>(Uri requestUrl, string namedClient, string bearerToken = null, IDictionary<string, string> headers = null)
        {
            return await CreateResponseAsync<T>(requestUrl, namedClient, HttpMethod.Get, null, bearerToken);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(Uri requestUrl, string namedClient, object jsonObject, string bearerToken, IDictionary<string, string> headers = null)
        {
            return await CreateResponseAsync<T>(requestUrl, namedClient, HttpMethod.Get, jsonObject, bearerToken, headers);
        }

        private async Task<ApiResponse<T>> CreateResponseAsync<T>(Uri requestUrl, string namedClient, HttpMethod httpMethod, object jsonObject, string bearerToken = null, IDictionary<string, string> headers = null)
        {
            var response = new ApiResponse<T>();
            try
            {
                Task<HttpResponseMessage> task = null;
                HttpResponseMessage httpResponseMessage = null;

                var client = CreateHttpClientWithHeaders(namedClient, bearerToken, headers);
                task = HttpCall(requestUrl, httpMethod, jsonObject, client);

                if (task == null)
                {
                    throw new NotImplementedException($"{httpMethod} method has not been implemented");
                }

                httpResponseMessage = await task;

                string responseString;
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    responseString = await ExtractDataToStringAsync(httpResponseMessage);
                    response.Data = JsonSerializer.Deserialize<T>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    response.WasSuccessful = true;
                    response.ResponseCode = HttpStatusCode.OK;
                }
                else
                {
                    responseString = await ExtractDataToStringAsync(httpResponseMessage);
                    logger.LogError(responseString);
                    response.Error = JsonSerializer.Deserialize<Error>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    response.WasSuccessful = false;
                    response.ResponseCode = HttpStatusCode.NotFound;
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"respose:{exception.Message}");
                response.WasSuccessful = false;
                response.ResponseCode = HttpStatusCode.InternalServerError;
                response.ResponseReason = HttpStatusCode.InternalServerError.ToString();
            }

            return response;
        }

        private static Task<HttpResponseMessage> HttpCall(Uri requestUrl, HttpMethod httpMethod, object jsonObject, HttpClient httpClient)
        {
            Task<HttpResponseMessage> task;

            if (httpMethod == HttpMethod.Get)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                task = httpClient.SendAsync(request);
            }
            else if (httpMethod == HttpMethod.Post)
            {
                var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, Application.Json);
                task = httpClient.PostAsync(requestUrl, content);
            }
            else
            {
                task = null;
            }

            return task;
        }

        private HttpClient CreateHttpClientWithHeaders(string namedClient, string bearerToken, IDictionary<string, string> headers = null)
        {
            var client = httpClientFactory.CreateClient(namedClient);

            if (bearerToken != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            if (headers != null && headers.Any())
            {
                foreach (KeyValuePair<string, string> kvp in headers)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            return client;
        }

        private static async Task<string> ExtractDataToStringAsync(HttpResponseMessage httpResponseMessage)
        {
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }
    }
}