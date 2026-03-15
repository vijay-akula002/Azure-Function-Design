using System.Xml.Linq;

namespace Common;

public interface IActionLabelHandler
{
    string BackendUri { get; }
    XDocument MapRequest(XDocument incomingSoap);

    XDocument MapResponse(XDocument backendResponse);
}