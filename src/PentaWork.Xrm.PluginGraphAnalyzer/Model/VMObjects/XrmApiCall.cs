namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class XrmApiCall : IVMObj
    {
        public string? Message { get; set; }
        public EntityObj? EntityInfo { get; set; }

        public bool IsTarget { get; set; } = false;
        public bool IsExecuted { get; set; } = true;

        /// <summary>
        /// Custom API / OrganizationRequest calls have no target entity, so the "- {entity}" segment
        /// and the "not executed" marker are only appended when there's actually something to show -
        /// otherwise they'd leave a dangling "- Message - " with nothing after the dash.
        /// </summary>
        public string DisplayLine
        {
            get
            {
                var line = $"- {Message}";
                if (EntityInfo?.LogicalName != null) line += $" - {EntityInfo.LogicalName}";
                if (!IsExecuted) line += " *(not executed)*";
                return line;
            }
        }
        public string UsedFieldsLine => EntityInfo != null ? $"- **Used Attributes**: {string.Join(", ", EntityInfo.UsedFields)}" : string.Empty;
        public bool HasUsedFields => EntityInfo?.UsedFields?.Any() == true;
        public bool HasCallLoopHit => EntityInfo?.CallLoopHit == true;
        public bool HasIsTarget => EntityInfo?.IsTarget == true;
    }
}
