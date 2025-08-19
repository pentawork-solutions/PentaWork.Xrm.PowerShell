using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects
{
    public class PluginInfo
    {
        public Guid Id { get; set; }
        public string? PlugintypeExportKey { get; set; }
        public string? TypeName { get; set; }
        public AssemblyInfo? AssemblyInfo { get; set; }
        public PackageInfo? PackageInfo { get; set; }
        public List<XrmApiCall>? ApiCalls { get; set; }
    }
}
