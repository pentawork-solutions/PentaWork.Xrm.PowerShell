using System.Xml;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class FormInfo
    {
        public FormInfo(Entity systemForm)
        {
            Name = systemForm["name"] as string;
            Document = new XmlDocument();
            Document.LoadXml(systemForm["formxml"] as string);
            TabNameList = GetTabNameList();
        }

        private List<string> GetTabNameList()
        {
            var tabNameList = new List<string>();
            var tabList = FormNode.SelectNodes("tabs/tab");
            foreach (XmlNode tab in tabList)
            {
                if (tab.Attributes["name"] != null)
                {
                    tabNameList.Add(tab.Attributes["name"].Value);
                }
            }
            return tabNameList;
        }

        public string Name { get; }
        public List<string> TabNameList { get; }

        public XmlDocument Document { get; }
        public XmlNode FormNode => Document.SelectSingleNode("form");
    }
}
