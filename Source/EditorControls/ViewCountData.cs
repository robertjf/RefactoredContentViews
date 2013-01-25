using System.Xml;
using umbraco.cms.businesslogic.datatype;

namespace Refactored.UmbracoViewCounter.EditorControls
{
    public class ViewCountData : DefaultData
    {
        public ViewCountData(BaseDataType datatype) : base(datatype) { }

        public override XmlNode ToXMl(XmlDocument data)
        {
            if (!string.IsNullOrEmpty((string)Value))
            {
                try
                {
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(Value.ToString());
                    return data.ImportNode(xd.DocumentElement, true);
                }
                catch (XmlException xe)
                {
                    throw new XmlException(string.Format("Error parsing Value: {0}", Value), xe);
                }
            }
            else
            {
                return base.ToXMl(data);
            }
        }
    }
}
