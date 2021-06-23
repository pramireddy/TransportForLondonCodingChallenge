using Lab.TfL.Dtos;
using System.Collections.Generic;
using Tfl.Api.Presentation.Entities;

namespace Lab.TfL.RestApi.Client.UnitTests.TestData
{
    public static class TestDataHelpers
    {
        public static IEnumerable<RoadCorridor> GetRoadCorridors()
        {
            return new List<RoadCorridor>
            {
                new RoadCorridor
                {
                    Id = "A406",
                    DisplayName = "North Circular (A406)",
                    StatusSeverityDescription = "Closure",
                    StatusSeverity = "Closure",
                    Url = "/Road/A406"
                },
                new RoadCorridor
                {
                    Id = "A2",
                    DisplayName = "A2",
                    StatusSeverityDescription = "No Exceptional Delays",
                    StatusSeverity = "Good",
                    Url = "/Road/A2"
                },
            };
        }

        public static Error GetApiError(string roadId)
        {
            return new Error
            {
                HttpStatusCode = 404,
                HttpStatus = "Not Found",
                Message = $"The following road id is not recognised: {roadId}"
            };
        }
    }
}