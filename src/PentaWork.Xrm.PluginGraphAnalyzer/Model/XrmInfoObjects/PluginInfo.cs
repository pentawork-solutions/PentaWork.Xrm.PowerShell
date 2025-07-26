namespace PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects
{
    public class PluginInfo
    {
        public Guid Id { get; set; }
        public string? PlugintypeExportKey { get; set; }
        public string? TypeName { get; set; }
        public string? AssemblyName { get; set; }
        public string? PackageName { get; set; }
        public Guid? PackageFileId { get; set; }
    }
}
