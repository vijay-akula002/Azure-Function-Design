using System.Xml.Linq;
using NameService;

namespace A
{
    public class RequestMapper
    {
        public static XDocument Map(string incomingReq)
        {
            var reqObj = (GetNamesRequest)Common.Utils.DeserializeSoapBody(incomingReq, typeof(GetNamesRequest));
            var backendReq = new GetANamesRequest();
            backendReq.header = new GetAHeader()
            {
                requestor = reqObj.header.user,
                requestDateTime = reqObj.header.dateTime,
                callingContext = reqObj.header.application
            };
            backendReq.filter = new GetAFilter()
            {
                includeNames = true,//reqObj.data.filter.includeNames,
                includeTrees = true,//reqObj.data.filter.includeTrees,
                includeRoads = true,//reqObj.data.filter.includeRoads

            };
            return XDocument.Parse(Common.Utils.SerializeToSoapEnvelope(backendReq));

        }
    }
}