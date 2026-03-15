using System.Xml.Linq;
using Common;
using Microsoft.Extensions.Configuration;
using NamesFunctionApp.Functions.A.Services;

namespace A
{
    [Action(ActionNames.A)]
    public class AHandler : IActionLabelHandler
    {
        public string BackendUri { get; }

        public AHandler(IConfiguration config)
        {
            BackendUri = config["Services:A:URI"] ?? throw new InvalidOperationException("Backend URI for the Action is not configured");
        }
        public XDocument MapRequest(XDocument incomingReq)
        {
            // Implement the logic to map the incoming request to the backend service request format
            return RequestMapper.Map(incomingReq.ToString());
        }

        public XDocument MapResponse(XDocument backendResp)
        {
            return ResponseMapper.Map(backendResp.ToString());
            // Implement the logic to map the backend service response to the outgoing response format
        }
    }
}