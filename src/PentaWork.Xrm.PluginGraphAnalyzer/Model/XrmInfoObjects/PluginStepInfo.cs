namespace PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects
{
    public class PluginStepInfo
    {
        public Guid Id { get; set; }
        public int? Rank { get; set; }
        public bool Async { get; set; }
        public bool Active { get; set; }
        public bool AsyncAutoDelete { get; set; }
        public string? Name { get; set; }
        public Stage Stage { get; set; }
        public string? Category { get; set; }
        public string? SdkMessage { get; set; }
        public List<string>? FilteringAttributes { get; set; }
        public string? PrimaryEntityName { get; set; }
        public string? SecondaryEntityName { get; set; }
        public PluginInfo? Plugin { get; set; }
    }
}
