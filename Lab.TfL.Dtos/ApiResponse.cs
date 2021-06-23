using System.Net;

namespace Lab.TfL.Dtos
{
    public class ApiResponse<T>
    {
        public bool WasSuccessful { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
        public string ResponseReason { get; set; }
        public T Data { get; set; }
        public Error Error { get; set; }
    }
}