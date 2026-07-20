using PentaWork.Xrm.PowerShell.XrmProxies;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class FormTabInfo
    {
        public FormTabInfo(string name)
        {
            Name = name;
            UniqueName = name.AsValidVariableName();
        }

        public string Name { get; }
        public string UniqueName { get; }
    }
}
