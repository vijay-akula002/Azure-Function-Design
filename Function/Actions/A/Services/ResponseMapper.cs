using System.Xml.Linq;
using A;
using NameService;

namespace NamesFunctionApp.Functions.A.Services
{
    public class ResponseMapper
    {

        public static XDocument Map(String backendResponse)
        {
            var resObj = (GetANamesResponse)Common.Utils.DeserializeSoapBody(backendResponse, typeof(GetANamesResponse));
            var backendResp = new GetNamesResponse()
            {
                header = new GetNamesResponseHeader()
                {
                    dateTime = resObj.header.requestDateTime,
                    correlationId = resObj.header.callingContext
                },
                data = new GetNamesResponseData()
                {
                    Names = MapNames(resObj.Names)
                }
            };
            return XDocument.Parse(Common.Utils.SerializeToSoapEnvelope(backendResp));
        }
        private static GetNamesResponseDataCategory[] MapNames(GetAResponseCategory[] categories)
        {
            var mapped = new GetNamesResponseDataCategory[categories.Length];

            for (int i = 0; i < categories.Length; i++)
            {
                var cat = categories[i];

                mapped[i] = new GetNamesResponseDataCategory
                {
                    CategoryName = cat.CategoryName,
                    Name = cat.Name?.ToArray() ?? Array.Empty<string>()
                };
            }

            return mapped;
        }
    }
}