using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Common.Extensions;

namespace NamesFunctionApp;

public class NamesServiceFunction
{
    private readonly IServiceProvider _services;
    public NamesServiceFunction(IServiceProvider services)
    {
        _services = services;
    }

    [Function("NamesServiceFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "names/v1")] HttpRequestData req,
        CancellationToken ct, FunctionContext context)
    {
        var kernel = _services.GetRequiredService<TECFunctionKernel>();
        return await kernel.ExecuteAsync(req, ct);
    }
}
