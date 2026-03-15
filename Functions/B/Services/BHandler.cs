using System.Xml.Linq;
using Common;
using Microsoft.Extensions.Configuration;


namespace B
{
    [Action(ActionNames.B)]
    public class BHandler : IActionLabelHandler
    {
        public string BackendUri { get; }

        public BHandler(IConfiguration config)
        {
            BackendUri = config["Services:B:URI"] ?? throw new InvalidOperationException("Backend URI for the Action is not configured");
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