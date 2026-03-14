using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NamesFunctionApp.Services
{
    public interface INamesBackendClient
    {
        Task<string> SendSoapAsync(string soap, String? uri, CancellationToken ct = default);
    }
    public class NamesBackendClient : INamesBackendClient
    {
        private readonly HttpClient _httpClient;

        public NamesBackendClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri(config["Services:BaseURL"] ?? throw new InvalidOperationException("Services:BaseURL configuration is missing"));

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> SendSoapAsync(string soap, String? uri, CancellationToken ct = default)
        {
            using var content = new StringContent(soap, Encoding.UTF8, "text/xml");
            using var response = await _httpClient.PostAsync(uri, content, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
    }
}
