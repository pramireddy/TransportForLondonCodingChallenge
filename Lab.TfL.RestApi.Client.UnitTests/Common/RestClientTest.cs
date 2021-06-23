using FluentAssertions;
using Lab.TfL.RestApi.Client.Common;
using Lab.TfL.RestApi.Client.UnitTests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tfl.Api.Presentation.Entities;
using Xunit;
using static System.Net.Mime.MediaTypeNames;

namespace Lab.TfL.RestApi.Client.UnitTests.Common
{
    public class RestClientTest
    {
        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private readonly Mock<ILogger<RestClient>> mockLogger;

        public RestClientTest()
        {
            mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockLogger = new Mock<ILogger<RestClient>>();
        }

        [Theory]
        [InlineData("A2")]
        [InlineData("A406")]
        public async Task WhenGivenAValidHttpClientGetAsyncReturnRoadStatus(string roadId)
        {
            //arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(TestDataHelpers.GetRoadCorridors()), Encoding.UTF8, Application.Json)
                });

            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                      .Returns(new HttpClient(mockHttpMessageHandler.Object));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://test.com/road")
            };
            httpClient.DefaultRequestHeaders.Clear();

            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var restClient = new RestClient(mockHttpClientFactory.Object, mockLogger.Object);

            // act
            var apiResponse = await restClient.GetAsync<IEnumerable<RoadCorridor>>(new Uri("https://test.com/road"), "Road");

            //assert
            apiResponse.WasSuccessful.Should().BeTrue();
            apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Data.Should().HaveCount(2);

            var roadStatus = apiResponse.Data.FirstOrDefault(x => x.Id == roadId);
            roadStatus.Should().NotBeNull();
            roadStatus.Id.Should().Be(roadId);
        }

        [Theory]
        [InlineData("PR2")]
        [InlineData("PR406")]
        public async Task WhenGivenAValidHttpClientGetAsyncReturnRoadStatusWithError(string roadId)
        {
            //arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(JsonSerializer.Serialize(TestDataHelpers.GetApiError(roadId)), Encoding.UTF8, Application.Json)
                });

            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                      .Returns(new HttpClient(mockHttpMessageHandler.Object));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://test.com/road")
            };
            httpClient.DefaultRequestHeaders.Clear();

            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var restClient = new RestClient(mockHttpClientFactory.Object, mockLogger.Object);

            // act
            var apiResponse = await restClient.GetAsync<IEnumerable<RoadCorridor>>(new Uri("https://test.com/road"), "Road");

            //assert
            apiResponse.WasSuccessful.Should().BeFalse();
            apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
            apiResponse.Data.Should().Equal(null);
            apiResponse.Error.Message.Should().Contain(roadId);
            apiResponse.Error.HttpStatus.Should().Be("Not Found");
        }
    }
}