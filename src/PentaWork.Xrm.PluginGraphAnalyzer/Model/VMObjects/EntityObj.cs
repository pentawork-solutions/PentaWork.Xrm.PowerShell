namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class EntityObj : IVMObj
    {
        public bool IsTarget { get; set; }
        public string? LogicalName { get; set; }
        public List<string> UsedFields { get; set; } = new();
        public bool CallLoopHit { get; set; } = false;
    }
}
