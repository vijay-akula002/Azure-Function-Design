using System.Xml.Linq;
using Common;
using Microsoft.Extensions.Configuration;
using NamesFunctionApp.Functions.A.Services;

namespace A
{
    [Action("A")]
    public class AHandler : IAction
    {
        public string BackendUri { get; }
        public string BackendName => "AzureAPIM"; 

        public AHandler(IConfiguration config)
        {
            BackendUri = config["Services:A:URI"] ?? throw new InvalidOperationException("Backend URI for the Action is not configured");
        }
        public XDocument MapRequest(XDocument incomingReq)
        {
            return RequestMapper.Map(incomingReq.ToString());
        }

        public XDocument MapResponse(string backendResp)
        {
            return ResponseMapper.Map(backendResp.ToString());
        }
    }
}