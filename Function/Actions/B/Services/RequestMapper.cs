using System.Xml.Linq;
using B;
using NameService;

namespace B
{
    public class RequestMapper
    {
        public static XDocument Map(string incomingSoap)
        {
            var reqObj = (GetNamesRequest)Common.Utils.DeserializeSoapBody(incomingSoap, typeof(GetNamesRequest));
            var backendReq = new GetBNamesRequest();
            backendReq.header = new GetBHeader()
            {
                requestor = reqObj.header.user,
                requestDateTime = reqObj.header.dateTime,
                callingContext = reqObj.header.application
            };
            backendReq.filter = new GetBFilter()
            {
                includeNames = true,//reqObj.data.filter.includeNames,
                includeTrees = true,//reqObj.data.filter.includeTrees,
                includeRoads = true,//reqObj.data.filter.includeRoads

            };
            return XDocument.Parse(Common.Utils.SerializeToSoapEnvelope(backendReq));
        }
    }
}