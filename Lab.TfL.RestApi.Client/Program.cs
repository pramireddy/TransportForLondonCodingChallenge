using Lab.TfL.Dtos;
using Lab.TfL.RestApi.Client.Common;
using Lab.TfL.RestApi.Client.Config;
using Lab.TfL.RestApi.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tfl.Api.Presentation.Entities;
using static System.Environment;

namespace Lab.TfL.RestApi.Client
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
                return ExitCode = 1;

            IHost host = CreateHostBuilder();

            try
            {
                var roadService = host.Services.GetRequiredService<IRoadService>();
                var apiResponse = await roadService.GetRoadById(args);

                return PrintRoadStatus(args, apiResponse);
            }
            catch (Exception exception)
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError($"TFL Road Corridors: Failed to get road status: {exception.Message}");
                return ExitCode = 1;
            }
        }

        private static int PrintRoadStatus(string[] roadIds, ApiResponse<IEnumerable<RoadCorridor>> result)
        {
            switch (result.ResponseCode)
            {
                case HttpStatusCode.OK:
                    foreach (var roadCorridor in result.Data)
                    {
                        Console.WriteLine($"The status of the {roadCorridor.DisplayName} is as follows");
                        Console.WriteLine($"\t Road status is {roadCorridor.StatusSeverity}");
                        Console.WriteLine($"\t Road status Description is {roadCorridor.StatusSeverityDescription}");
                    }
                    return ExitCode = 0;

                case HttpStatusCode.NotFound:
                    Console.WriteLine($"{result.Error.Message}");
                    return ExitCode = 1;

                default:
                    Console.WriteLine($"Fail to get Road status for {string.Join(",", roadIds)}");
                    return ExitCode = 1;
            }
        }

        private static IHost CreateHostBuilder()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();

            var config = configurationBuilder.Build();

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient("Road")
                            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                            .AddPolicyHandler(GetRetryPolicy());

                    services.Configure<TfLApiSettings>(config.GetSection(nameof(TfLApiSettings)));
                    services.AddTransient<IRestClient, RestClient>();
                    services.AddTransient<IRoadService, RoadService>();
                }).UseConsoleLifetime();

            var host = builder.Build();
            return host;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)));
        }
    }
}