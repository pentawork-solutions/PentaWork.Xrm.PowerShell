using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    public class EntityGraphList : List<EntityGraph>
    {
        private readonly Dictionary<string, List<XrmApiCall>> _apiCalls;

        public EntityGraphList(Dictionary<string, List<XrmApiCall>> apiCalls)
        {
            _apiCalls = apiCalls;
        }

        public void Add(PluginStepInfo pluginStepInfo)
        {
            if (pluginStepInfo.Plugin != null)
                pluginStepInfo.Plugin.ApiCalls = _apiCalls.SingleOrDefault(c => c.Key == pluginStepInfo.Plugin.TypeName).Value;

            var entity = this.SingleOrDefault(e => e.EntityName == pluginStepInfo.PrimaryEntityName);
            if (entity == null)
            {
                entity = new EntityGraph(pluginStepInfo.PrimaryEntityName) { Owner = this };
                Add(entity);
            }
            entity.Add(pluginStepInfo);
        }

        /// <summary>
        /// Cross-references every step's outgoing API calls (message + target entity) against
        /// every step's own registration (SdkMessage + PrimaryEntityName) across the whole
        /// system, populating <see cref="PluginStepInfo.TriggeredSteps"/>/<see cref="PluginStepInfo.TriggeredBy"/>.
        /// Call once after all steps have been added - this is a report-time heuristic, not part
        /// of the IL analysis, so it needs the full picture of every entity's steps at once.
        /// </summary>
        public void LinkTriggers()
        {
            var allSteps = this
                .SelectMany(g => g.Messages)
                .SelectMany(m => m.Stages.Values)
                .SelectMany(steps => steps)
                .ToList();

            var stepsByTrigger = allSteps
                .Where(s => s.SdkMessage != null && s.PrimaryEntityName != null)
                .GroupBy(s => (Message: s.SdkMessage!.ToLowerInvariant(), Entity: s.PrimaryEntityName!))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var step in allSteps)
            {
                var links = new List<TriggerLink>();
                foreach (var apiCall in step.ApiCalls)
                {
                    if (apiCall.Message == null || apiCall.EntityInfo?.LogicalName == null) continue;

                    var key = (Message: apiCall.Message.ToLowerInvariant(), Entity: apiCall.EntityInfo.LogicalName);
                    if (!stepsByTrigger.TryGetValue(key, out var matchingSteps)) continue;

                    foreach (var matchedStep in matchingSteps)
                    {
                        if (!MatchesFilteringAttributes(matchedStep, apiCall)) continue;
                        links.Add(new TriggerLink(matchedStep, apiCall.Message, apiCall.EntityInfo.LogicalName, matchedStep.Id == step.Id));
                    }
                }

                step.TriggeredSteps = links.GroupBy(l => l.OtherStep.Id).Select(g => g.First()).ToList();
            }

            foreach (var step in allSteps)
            {
                foreach (var link in step.TriggeredOtherSteps)
                    link.OtherStep.TriggeredBy.Add(new TriggerLink(step, link.Message, link.EntityName, isSelf: false));
            }
        }

        /// <summary>
        /// A step registered with FilteringAttributes only actually fires when the triggering call
        /// touches at least one of those attributes (this is how Dataverse itself decides whether
        /// to invoke an Update step) - without this check, "Triggers"/"Triggered by" would list
        /// steps that would never really run for that specific call. A step with no filtering
        /// attributes at all fires on every matching message/entity call, unconditionally.
        /// </summary>
        private static bool MatchesFilteringAttributes(PluginStepInfo candidate, XrmApiCall apiCall)
        {
            if (candidate.FilteringAttributes == null || candidate.FilteringAttributes.Count == 0) return true;

            var touchedFields = apiCall.EntityInfo?.UsedFields;
            if (touchedFields == null || touchedFields.Count == 0) return false;

            return candidate.FilteringAttributes.Any(attr => touchedFields.Contains(attr, StringComparer.OrdinalIgnoreCase));
        }
    }
}
