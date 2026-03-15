using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace Common;

public interface ITECBackendSoapClient
{
    string BackendName { get; }
    Task<string> CallAsync(XDocument request, string? uri, CancellationToken ct = default);
}

public class AzureAPIMClient : ITECBackendSoapClient
{
    private readonly HttpClient _httpClient;

    public string BackendName => "AzureAPIM";

    public AzureAPIMClient()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:8088/namesservice/"); //TODO: Make this configurable
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }
    public AzureAPIMClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(config["Services:BaseURL"] ?? throw new InvalidOperationException("Services:BaseURL configuration is missing"));

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }
    public async Task<string> CallAsync(XDocument request, string? uri, CancellationToken ct = default)
    {
        using var content = new StringContent(request.ToString(), Encoding.UTF8, "text/xml");
        using var response = await _httpClient.PostAsync(uri, content, ct);//.ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }
}