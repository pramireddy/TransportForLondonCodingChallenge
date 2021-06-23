using Lab.TfL.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tfl.Api.Presentation.Entities;

namespace Lab.TfL.RestApi.Client.Services
{
    public interface IRoadService
    {
        Task<ApiResponse<IEnumerable<RoadCorridor>>> GetRoadById(params string[] roadIds);
    }
}