using System;
using System.Xml;

namespace S2HD.Graphics
{
    public static class XmlExtensions
    {
        public static string GetNodeInnerText(this XmlNode node, string nodeName, string defaultValue = "")
        {
            var childNode = node.SelectSingleNode(nodeName);
            return childNode?.InnerText ?? defaultValue;
        }

        public static bool TryGetNodeInnerText(this XmlNode node, string nodeName, out string value)
        {
            var childNode = node.SelectSingleNode(nodeName);
            if (childNode != null)
            {
                value = childNode.InnerText;
                return true;
            }
            value = null;
            return false;
        }
    }
}
