using System.Net;

namespace UserManagementApp.Tests.Core.Clients;

public class ApiTestResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }

    public T? Body { get; set; }

    public string RawBody { get; set; } = string.Empty;

    public bool IsSuccessStatusCode { get; set; }
}