namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class EntityObj : IVMObj
    {
        public string? LogicalName { get; set; }
        public List<string> UsedFields { get; set; } = new();
    }
}
