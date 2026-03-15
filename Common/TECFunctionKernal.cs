using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
namespace Common.Extensions;

public class TECFunctionKernel
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<TECFunctionKernel> _logger;

    public TECFunctionKernel(IServiceProvider provider, ILogger<TECFunctionKernel> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<HttpResponseData> ExecuteAsync(HttpRequestData req, CancellationToken ct)
    {
        try
        {
            var incomingDoc = await XDocument.LoadAsync(req.Body, LoadOptions.None, ct);
            _logger.LogInformation("Received Client Request:{Soap}:l", @incomingDoc.ToString());
            var action = ExtractAction(incomingDoc);
            var actionHandler = _provider.GetRequiredKeyedService<IAction>(action); //Should we dictionary lookup to avoid container access twice?
            var backendRequest = actionHandler.MapRequest(incomingDoc);
            _logger.LogInformation("Sending Backend Request: {BackendRequest}.", backendRequest.ToString());
            var backendClient = _provider.GetRequiredKeyedService<ITECBackendSoapClient>(actionHandler.BackendName); //Should we dictionary lookup to avoid container access twice?
            string backendResponse = await backendClient.CallAsync(backendRequest, actionHandler.BackendUri, ct);
            _logger.LogInformation("Response Backend Received: {BackendResponse}.", backendResponse);
            var soapRespToClient = actionHandler.MapResponse(backendResponse);
            _logger.LogInformation("Sending Client Response: {soapRespToClient}.", soapRespToClient);
            return await Common.Utils.ReturnResponse(req, soapRespToClient.ToString(), ct);
        }
        catch (Exception ex) //Identify IIB SOAP Faults and implement them here
        {
            _logger.LogError(ex, "{name} Function failed while processing SOAP request. Error: {message}", req.FunctionContext.FunctionDefinition.Name, ex.Message);
            var error = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            error.Headers.Add("Content-Type", "text/plain");
            await error.WriteStringAsync("Internal Server Error", ct);
            string fault = Common.Utils.CreateSoapFault("Server", "An error occurred while processing the request.");
            return await Utils.ReturnResponse(req, fault, ct);
        }
    }

    private string ExtractAction(XDocument doc)
    {
        return doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "action")?.Value ?? string.Empty;
    }
}