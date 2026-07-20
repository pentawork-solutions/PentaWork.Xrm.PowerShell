using PentaWork.Xrm.PluginGraph.Model.GraphObjects;

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

        /// <summary>Other registered steps that this step's own API calls would trigger, populated by <see cref="EntityGraphList.LinkTriggers"/>. Includes a self-entry when the step re-triggers its own message+entity.</summary>
        public List<TriggerLink> TriggeredSteps { get; set; } = new();

        /// <summary>Other steps whose API calls would trigger this one (the reverse of <see cref="TriggeredSteps"/>).</summary>
        public List<TriggerLink> TriggeredBy { get; set; } = new();

        public string FilteringAttributesLine => FilteringAttributes != null
            ? $"- **Filtering Attributes**: {string.Join(", ", FilteringAttributes)}"
            : string.Empty;
        public List<Model.VMObjects.XrmApiCall> ApiCalls => Plugin?.ApiCalls ?? new();

        /// <summary>
        /// Scriban's reflection binding doesn't resolve List&lt;T&gt;.Count (it silently reads as
        /// null/empty), so precompute this rather than writing "ApiCalls.Count > 0" in a template.
        /// </summary>
        public bool HasApiCalls => ApiCalls.Count > 0;

        /// <summary>Markdown anchor id used to deep-link into this step from other entities' reports.</summary>
        public string AnchorId => $"step-{Id}";
        public string EntityFileName => $"{PrimaryEntityName}.md";
        public string ModeLabel => Async ? "Async" : "Sync";
        public string StageName => Stage.ToString();
        public string RankLabel => Rank?.ToString() ?? "(default)";

        public TriggerLink? SelfTrigger => TriggeredSteps.FirstOrDefault(t => t.IsSelf);
        public bool HasSelfTrigger => SelfTrigger != null;
        public List<TriggerLink> TriggeredOtherSteps => TriggeredSteps.Where(t => !t.IsSelf).ToList();
        public bool HasTriggeredOtherSteps => TriggeredOtherSteps.Count > 0;
        public bool HasTriggeredBy => TriggeredBy.Count > 0;

        private const int MaxTriggerLinksShown = 50;

        public List<TriggerLink> TriggeredOtherStepsShown => TriggeredOtherSteps.Take(MaxTriggerLinksShown).ToList();
        public int TriggeredOtherStepsOverflowCount => Math.Max(0, TriggeredOtherSteps.Count - MaxTriggerLinksShown);
        public bool HasTriggeredOtherStepsOverflow => TriggeredOtherStepsOverflowCount > 0;

        public List<TriggerLink> TriggeredByShown => TriggeredBy.Take(MaxTriggerLinksShown).ToList();
        public int TriggeredByOverflowCount => Math.Max(0, TriggeredBy.Count - MaxTriggerLinksShown);
        public bool HasTriggeredByOverflow => TriggeredByOverflowCount > 0;
    }
}
