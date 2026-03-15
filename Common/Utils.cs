namespace Common
{
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Collections.Concurrent;
    using Microsoft.Azure.Functions.Worker.Http;
    using System.Net;

    public static class Utils
    {
        private const string SoapEnvNs = "http://schemas.xmlsoap.org/soap/envelope/";

        private static readonly XmlWriterSettings s_writerSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = false
        };

        // Cache XmlSerializers because construction is expensive
        private static readonly ConcurrentDictionary<string, XmlSerializer> s_serializerCache = new();

        // -----------------------------
        // DESERIALIZATION
        // -----------------------------
        public static object DeserializeSoapBody(string soapXml, Type targetType)
        {
            if (soapXml is null) throw new ArgumentNullException(nameof(soapXml));
            if (targetType is null) throw new ArgumentNullException(nameof(targetType));

            using var sr = new StringReader(soapXml);
            using var xr = XmlReader.Create(sr);

            // Advance to the Body element in the SOAP namespace
            while (xr.Read())
            {
                if (xr.NodeType == XmlNodeType.Element && xr.LocalName == "Body" && xr.NamespaceURI == SoapEnvNs)
                {
                    break;
                }
            }

            if (xr.EOF) throw new InvalidOperationException("SOAP Envelope/Body not found.");

            // Move to the first element inside Body
            if (!xr.Read()) throw new InvalidOperationException("SOAP Body is empty.");
            while (xr.NodeType != XmlNodeType.Element && xr.Read()) { }
            if (xr.NodeType != XmlNodeType.Element) throw new InvalidOperationException("SOAP Body contains no element to deserialize.");

            var localName = xr.LocalName;
            var ns = xr.NamespaceURI ?? string.Empty;

            var xmlRoot = new XmlRootAttribute(localName) { Namespace = ns };
            var serializer = GetOrAddSerializer(targetType, xmlRoot);

            using var sub = xr.ReadSubtree();
            sub.Read(); // position on the element
            var result = serializer.Deserialize(sub);
            return result ?? throw new InvalidOperationException("Failed to deserialize SOAP body.");
        }

        // -----------------------------
        // SERIALIZATION
        // -----------------------------
        public static string SerializeToSoapEnvelope(object obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();
            var mc = (MessageContractAttribute?)Attribute.GetCustomAttribute(type, typeof(MessageContractAttribute));
            if (mc == null) throw new InvalidOperationException($"{type.Name} is not a MessageContract type.");

            var rootName = mc.WrapperName ?? type.Name;
            var rootNs = mc.WrapperNamespace ?? string.Empty;
            var xmlRoot = new XmlRootAttribute(rootName) { Namespace = rootNs };

            var serializer = GetOrAddSerializer(type, xmlRoot);

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, s_writerSettings))
            {
                var ns = new XmlSerializerNamespaces();
                if (!string.IsNullOrEmpty(rootNs)) ns.Add(string.Empty, rootNs);
                serializer.Serialize(writer, obj, ns);
            }

            return WrapInSoapEnvelope(sb.ToString());
        }

        // -----------------------------
        // SOAP ENVELOPE BUILDER
        // -----------------------------
        public static string WrapInSoapEnvelope(string bodyXml)
        {
            if (bodyXml is null) throw new ArgumentNullException(nameof(bodyXml));
            // Build compact envelope without extraneous whitespace
            return string.Concat("<soapenv:Envelope xmlns:soapenv=\"", SoapEnvNs, "\">",
                                 "<soapenv:Body>", bodyXml, "</soapenv:Body>",
                                 "</soapenv:Envelope>");
        }

        // -----------------------------
        // SOAP INVOCATION
        // -----------------------------
        public static async Task<string> SoapInvokeAsync(string endpoint, string soapXml, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (endpoint is null) throw new ArgumentNullException(nameof(endpoint));
            if (soapXml is null) throw new ArgumentNullException(nameof(soapXml));
            if (httpClient is null) throw new ArgumentNullException(nameof(httpClient));

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(soapXml, Encoding.UTF8, "text/xml")
            };

            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        private static XmlSerializer GetOrAddSerializer(Type type, XmlRootAttribute root)
        {
            var key = string.Concat(type.AssemblyQualifiedName ?? type.FullName, "|", root.ElementName, "|", root.Namespace);
            return s_serializerCache.GetOrAdd(key, _ => root is null ? new XmlSerializer(type) : new XmlSerializer(type, root));
        }

        public static async Task<HttpResponseData> ReturnResponse(HttpRequestData req, string soapBody, CancellationToken ct = default)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            await response.WriteStringAsync(soapBody, ct);
            return response;
        }

        public static string CreateSoapFault(string faultCode, string faultString)
        {
            if (faultCode is null) throw new ArgumentNullException(nameof(faultCode));
            if (faultString is null) throw new ArgumentNullException(nameof(faultString));

            // Escape values to avoid injecting malformed XML
            var codeEscaped = WebUtility.HtmlEncode(faultCode);
            var stringEscaped = WebUtility.HtmlEncode(faultString);

            var sb = new StringBuilder();
            sb.Append("<soapenv:Envelope xmlns:soapenv=\"").Append(SoapEnvNs).Append("\">")
              .Append("<soapenv:Body>")
              .Append("<soapenv:Fault>")
              .Append("<faultcode>").Append(codeEscaped).Append("</faultcode>")
              .Append("<faultstring>").Append(stringEscaped).Append("</faultstring>")
              .Append("</soapenv:Fault>")
              .Append("</soapenv:Body>")
              .Append("</soapenv:Envelope>");

            return sb.ToString();
        }

        public static async Task<HttpResponseData> ReturnSoapFaultResponse(HttpRequestData req, string faultCode, string faultString, HttpStatusCode status = HttpStatusCode.BadRequest, CancellationToken ct = default)
        {
            var response = req.CreateResponse(status);
            response.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            var body = CreateSoapFault(faultCode, faultString);
            await response.WriteStringAsync(body, ct);
            return response;
        }
    }
}
