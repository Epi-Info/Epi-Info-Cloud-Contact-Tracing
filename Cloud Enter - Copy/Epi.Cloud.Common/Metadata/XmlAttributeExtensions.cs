using System.Xml.Linq;

namespace Epi.Cloud.Common.Metadata
{
    public static class XmlAttributeExtensions
    {
        public static string AttributeValue(this XElement fieldType, XName attrName, string defaultValue = null)
        {
            var attribute = fieldType.Attribute(attrName);
            return attribute != null ? attribute.Value : defaultValue;
        }
    }
}
