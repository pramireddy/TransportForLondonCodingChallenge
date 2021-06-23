using FluentAssertions;
using Lab.TfL.Dtos;
using Lab.TfL.RestApi.Client.Common;
using Lab.TfL.RestApi.Client.Config;
using Lab.TfL.RestApi.Client.Services;
using Lab.TfL.RestApi.Client.UnitTests.TestData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tfl.Api.Presentation.Entities;
using Xunit;

namespace Lab.TfL.RestApi.Client.UnitTests.Services
{
    public class RoadServiceTest
    {
        private readonly Mock<IRestClient> mockRestClient;
        private readonly Mock<ILogger<RoadService>> mockLogger;
        private readonly Mock<IOptions<TfLApiSettings>> mockTflApiSettings;

        public RoadServiceTest()
        {
            mockRestClient = new Mock<IRestClient>();
            mockLogger = new Mock<ILogger<RoadService>>();
            mockTflApiSettings = new Mock<IOptions<TfLApiSettings>>();

            mockTflApiSettings.Setup<TfLApiSettings>(x => x.Value)
                                .Returns(
                                new TfLApiSettings
                                {
                                    ApiBaseAddress = "https://api.tfl.gov.uk",
                                    RoadEndpoint = "/Road",
                                    ApiId = "test@test.com",
                                    ApiKey = "2fb095a9f3a44e8db55787eba36e1234"
                                });
        }

        [Theory]
        [InlineData("A2")]
        [InlineData("A406")]
        public async Task WhenGivenAValidRoadIdGetRoadByIdReturnRoadStatusWithStatusCode200(string roadId)
        {
            //arrange
            mockRestClient.Setup(x => x.GetAsync<IEnumerable<RoadCorridor>>(It.IsAny<Uri>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(
                new ApiResponse<IEnumerable<RoadCorridor>>
                {
                    ResponseCode = HttpStatusCode.OK,
                    WasSuccessful = true,
                    Data = TestDataHelpers.GetRoadCorridors().Where(x => x.Id == roadId)
                })); ;

            // act
            var roadService = new RoadService(mockRestClient.Object, mockTflApiSettings.Object, mockLogger.Object);
            var apiResponse = await roadService.GetRoadById(roadId);

            //assert
            apiResponse.WasSuccessful.Should().BeTrue();
            apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Data.Should().HaveCount(1);

            var roadStatus = apiResponse.Data.FirstOrDefault(x => x.Id == roadId);
            roadStatus.Should().NotBeNull();
            roadStatus.Id.Should().Be(roadId);

            mockRestClient.Verify(x => x.GetAsync<IEnumerable<RoadCorridor>>(It.IsAny<Uri>(), It.IsAny<string>(), null, null), Times.Once());
        }

        [Theory]
        [InlineData("PR2")]
        [InlineData("PR406")]
        public async Task WhenGivenAInvalidRoadIdGetRoadByIdReturnRoadStatusWithStatusCode404(string roadId)
        {
            //arrange
            mockRestClient.Setup(x => x.GetAsync<IEnumerable<RoadCorridor>>(It.IsAny<Uri>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(
                new ApiResponse<IEnumerable<RoadCorridor>>
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    WasSuccessful = false,
                    Data = null,
                    Error = TestDataHelpers.GetApiError(roadId)
                }));

            // act
            var roadService = new RoadService(mockRestClient.Object, mockTflApiSettings.Object, mockLogger.Object);
            var apiResponse = await roadService.GetRoadById(roadId);

            //assert
            apiResponse.WasSuccessful.Should().BeFalse();
            apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
            apiResponse.Data.Should().Equal(null);
            apiResponse.Error.Message.Should().Contain(roadId);
            apiResponse.Error.HttpStatus.Should().Be("Not Found");

            mockRestClient.Verify(x => x.GetAsync<IEnumerable<RoadCorridor>>(It.IsAny<Uri>(), It.IsAny<string>(), null, null), Times.Once());
        }
    }
}