namespace PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects
{
    public class PluginStepInfo
    {
        public Guid Id { get; set; }
        public int? Mode { get; set; }
        public int? Stage { get; set; }
        public int? Rank { get; set; }
        public int? StateCode { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? FilteringAttributes { get; set; }
        public PluginInfo? Plugin { get; set; }
        public SdkMessageInfo? SdkMessage { get; set; }
        public SdkFilterInfo? SdkFilter { get; set; }
    }
}
