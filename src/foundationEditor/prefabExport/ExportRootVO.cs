using System.Collections.Generic;
using System.Xml;
using foundation;

namespace foundationExport
{
    public class ExportRootVO:IXMLBinder
    {
        public string name;
        public List<string> from=new List<string>();
        public List<string> extentions=new List<string>();
        public string to;

        public void bindXML(XmlNode xml)
        {
            this.from.Clear();
            this.extentions.Clear();
            XmlAttribute attribute;

            attribute = xml.Attributes["name"];
            if (attribute != null)
            {
                this.name = attribute.InnerText;
            }
            attribute = xml.Attributes["from"];
            string[] list = attribute.InnerText.As3Split(",");
            foreach (string item in list)
            {
                string v = item;
                if (v.EndsWith("/") == false)
                {
                    v += "/";
                }

                this.from.Add(v);
            }

            attribute = xml.Attributes["extention"];
            if (attribute != null)
            {
                list = attribute.InnerText.As3Split(",");
                foreach (string s in list)
                {
                    extentions.Add("*." + s);
                }
            }
            if (extentions.Count == 0)
            {
                extentions.Add("*.prefab");
            }

        }
    }
}