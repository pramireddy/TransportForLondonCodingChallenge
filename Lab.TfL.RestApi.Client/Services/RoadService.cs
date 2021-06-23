using Lab.TfL.Dtos;
using Lab.TfL.RestApi.Client.Common;
using Lab.TfL.RestApi.Client.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tfl.Api.Presentation.Entities;

namespace Lab.TfL.RestApi.Client.Services
{
    public class RoadService : IRoadService
    {
        private readonly IRestClient restClient;
        private readonly TfLApiSettings tfLApiSettings;
        private readonly ILogger<RoadService> logger;

        public RoadService(IRestClient restClient, IOptions<TfLApiSettings> tfLApiSettings, ILogger<RoadService> logger)
        {
            this.restClient = restClient;
            this.tfLApiSettings = tfLApiSettings.Value;
            this.logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<RoadCorridor>>> GetRoadById(params string[] roadIds)
        {
            var relativeUri = $"{tfLApiSettings.RoadEndpoint}/{string.Join(",", roadIds)}";

            var uri = new Uri($"{tfLApiSettings.ApiBaseAddress}{relativeUri}?app_id={tfLApiSettings.ApiId}&app_key={tfLApiSettings.ApiKey}");

            logger.LogInformation($"Calling Road URI: {uri}");

            var response = await restClient.GetAsync<IEnumerable<RoadCorridor>>(uri,"Road");

            logger.LogInformation($"Received response status code: {response.ResponseCode}");

            return response;
        }
    }
}