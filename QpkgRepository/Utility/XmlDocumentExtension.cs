﻿using System.Xml;

namespace ArxOne.Qnap.Utility;

public static class XmlDocumentExtension
{
    public static XmlElement CreateAndAddElement(this XmlDocument xml, XmlNode parent, string elementName)
    {
        var element = xml.CreateElement(elementName);
        parent.AppendChild(element);
        return element;
    }

    public static void AddElement(this XmlDocument xml, XmlNode parent, string elementName, string elementValue)
    {
        var element = xml.CreateElement(elementName);
        element.InnerText = elementValue;
        parent.AppendChild(element);
    }
}