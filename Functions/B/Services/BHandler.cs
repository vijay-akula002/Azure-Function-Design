using System.Xml.Linq;
using Common;
using Microsoft.Extensions.Configuration;


namespace B
{
    [Action("B")]
    public class BHandler : IActionLabelHandler
    {
        public string BackendUri { get; }

        public BHandler(IConfiguration config)
        {
            BackendUri = config["Services:B:URI"] ?? throw new InvalidOperationException("Backend URI for the Action is not configured");
        }
        public XDocument MapRequest(XDocument incomingReq)
        {
            return RequestMapper.Map(incomingReq.ToString());
        }

        public XDocument MapResponse(XDocument backendResp)
        {
            return ResponseMapper.Map(backendResp.ToString());
        }
    }
}