using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using NamesFunctionApp.Services;
using System.Xml.Linq;
using Common;

namespace NamesFunctionApp;

public class NamesServiceFunction
{
    private readonly ILogger<NamesServiceFunction> _logger;
    private readonly INamesBackendClient _backendClient;
    private readonly HandlerResolver _resolver;
    public NamesServiceFunction(INamesBackendClient backendClient, ILogger<NamesServiceFunction> logger, HandlerResolver resolver)
    {
        _backendClient = backendClient;
        _logger = logger;
        _resolver = resolver;
    }

    [Function("NamesServiceFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "names/v1")] HttpRequestData req,
        CancellationToken ct)
    {
        try
        {
            var incomingDoc = await XDocument.LoadAsync(req.Body, LoadOptions.None, ct);
            _logger.LogInformation("Incoming SOAP Request:{Soap}", incomingDoc.ToString());
            var action = ExtractAction(incomingDoc);
            var handler = _resolver.Resolve(action);
            var backendRequest = handler.MapRequest(incomingDoc);
            _logger.LogInformation("Backend Request: {BackendRequest}.", backendRequest.ToString());
            string backendResponse = await _backendClient.SendSoapAsync(backendRequest.ToString(), handler.BackendUri, ct);
            _logger.LogInformation("Backend Response: {BackendResponse}.", backendResponse);
            var soapRespToClient = handler.MapResponse(XDocument.Parse(backendResponse));
            _logger.LogInformation("SOAP Response to Client: {soapRespToClient}.", soapRespToClient);
            return  await Common.Utils.ReturnResponse(req, soapRespToClient.ToString(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NamesServiceFunction failed while processing SOAP request. Error: {message}", ex.Message);
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            error.Headers.Add("Content-Type", "text/plain");
            await error.WriteStringAsync("Internal Server Error", ct);

            return error;
        }
    }

    private static string ExtractAction(XDocument doc)
    {
        XNamespace ns = "http://example.com/namesservice";

        return doc.Descendants(ns + "action").First().Value;
    }
}
