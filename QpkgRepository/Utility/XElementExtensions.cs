using System.Xml.Linq;

namespace ArxOne.Qnap.Utility;

internal static class XElementExtensions
{
    public static void AddXElementIfNotNullOrEmpty(this XElement xElement, XName name, string? value, bool asCData = false)
    {
        if (string.IsNullOrEmpty(value))
            return;
        xElement.Add(new XElement(name, asCData ? new XCData(value) : value));
    }
}
