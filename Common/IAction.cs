using System.Xml.Linq;

namespace Common;

public interface IAction
{
    string BackendUri { get; }
    string BackendName { get; }
    XDocument MapRequest(XDocument incomingSoap);

    XDocument MapResponse(string backendResponse);
}