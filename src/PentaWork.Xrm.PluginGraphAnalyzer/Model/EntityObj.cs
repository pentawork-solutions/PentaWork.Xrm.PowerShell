namespace PentaWork.Xrm.PluginGraph.Model
{
    public class EntityObj
    {
        public string? LogicalName { get; set; }
        public List<string> UsedFields { get; set; } = new();
    }
}
