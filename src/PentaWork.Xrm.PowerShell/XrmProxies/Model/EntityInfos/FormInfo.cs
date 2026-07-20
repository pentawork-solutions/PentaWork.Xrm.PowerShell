using System.Xml;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using PentaWork.Xrm.PowerShell.XrmProxies;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class FormInfo
    {
        public FormInfo(Entity systemForm)
        {
            Name = systemForm["name"] as string;
            UniqueName = Name.AsValidVariableName();
            Document = new XmlDocument();
            Document.LoadXml(systemForm["formxml"] as string);
            TabList = GetTabList();
        }

        private List<FormTabInfo> GetTabList()
        {
            var tabList = new List<FormTabInfo>();
            var tabNodeList = FormNode.SelectNodes("tabs/tab");
            foreach (XmlNode tab in tabNodeList)
            {
                if (tab.Attributes["name"] != null)
                {
                    tabList.Add(new FormTabInfo(tab.Attributes["name"].Value));
                }
            }
            return tabList;
        }

        public string Name { get; }
        public string UniqueName { get; }
        public List<FormTabInfo> TabList { get; }

        public XmlDocument Document { get; }
        public XmlNode FormNode => Document.SelectSingleNode("form");
    }
}
