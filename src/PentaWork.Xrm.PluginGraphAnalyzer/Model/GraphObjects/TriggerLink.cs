using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    /// <summary>
    /// One edge in the system-wide plugin trigger graph: "this step performs {Message} on
    /// {EntityName}, which is exactly what {TriggeredStep} is registered for." Built by
    /// <see cref="EntityGraphList.LinkTriggers"/> from data the analyzer already collects -
    /// no IL re-analysis needed.
    /// </summary>
    public class TriggerLink
    {
        public TriggerLink(PluginStepInfo otherStep, string message, string entityName, bool isSelf)
        {
            OtherStep = otherStep;
            Message = message;
            EntityName = entityName;
            IsSelf = isSelf;
        }

        /// <summary>In a <see cref="PluginStepInfo.TriggeredSteps"/> list this is the step being triggered; in <see cref="PluginStepInfo.TriggeredBy"/> it's the step doing the triggering.</summary>
        public PluginStepInfo OtherStep { get; }
        public string Message { get; }
        public string EntityName { get; }

        /// <summary>True if this link points back at the same step that made the API call.</summary>
        public bool IsSelf { get; }
    }
}
