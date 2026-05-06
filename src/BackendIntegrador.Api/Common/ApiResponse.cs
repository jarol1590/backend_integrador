using System.Net;

namespace BackendIntegrador.Api.Common;

public class ApiResponse<T>
{
    public bool success { get; set; }
    public HttpStatusCode status { get; set; }
    public string? method { get; set; }
    public string? errors { get; set; }
    public T? response { get; set; }
}