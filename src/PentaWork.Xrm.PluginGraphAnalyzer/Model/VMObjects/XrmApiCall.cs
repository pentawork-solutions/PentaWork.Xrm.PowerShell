namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class XrmApiCall : IVMObj
    {
        public string? Message { get; set; }
        public EntityObj? EntityInfo { get; set; }

        public bool IsTarget { get; set; } = false;
        public bool IsExecuted { get; set; } = true;

        public string DisplayLine => $"- {Message} - {EntityInfo?.LogicalName} {(!IsExecuted ? "*(not executed)*" : string.Empty)}";
        public string UsedFieldsLine => EntityInfo != null ? $"- **Used Attributes**: {string.Join(", ", EntityInfo.UsedFields)}" : string.Empty;
        public bool HasUsedFields => EntityInfo?.UsedFields?.Any() == true;
        public bool HasCallLoopHit => EntityInfo?.CallLoopHit == true;
        public bool HasIsTarget => EntityInfo?.IsTarget == true;
    }
}
