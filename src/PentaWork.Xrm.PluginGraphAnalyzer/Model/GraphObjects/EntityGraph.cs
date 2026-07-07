using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;
using PentaWork.Xrm.PluginGraph.Templates;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    public class EntityGraph
    {
        public EntityGraph(string entityName)
        {
            EntityName = entityName;
        }

        public void Add(PluginStepInfo pluginStepInfo)
        {
            var message = Messages.SingleOrDefault(e => e.Message == pluginStepInfo.SdkMessage);
            if (message == null)
            {
                message = new MessageGraph(pluginStepInfo.SdkMessage);
                Messages.Add(message);
            }
            message.Add(pluginStepInfo);
        }

        public string ToMarkdown()
        {
            return ScribanTemplateRenderer.Render("MainTemplate", new { EntityGraph = this });
        }

        public string EntityName { get; }
        public List<MessageGraph> Messages { get; } = new();
        public EntityGraphSummary Summary => new(this);
        public List<PluginStepInfo> AllSteps => Messages.SelectMany(m => m.Stages.Values).SelectMany(steps => steps).ToList();
        public EntityGraphList? Owner { get; internal set; }
    }

    /// <summary>
    /// Quick-glance overview rendered at the top of each entity's report, so a reader doesn't
    /// have to walk every stage/message section to spot a self-recursion risk or see which other
    /// entities this one's plugins reach into.
    /// </summary>
    public class EntityGraphSummary
    {
        private readonly HashSet<string> _entitiesWithOwnPage;

        public EntityGraphSummary(EntityGraph entityGraph)
        {
            var allSteps = entityGraph.AllSteps;

            // Only entities that themselves have at least one registered plugin step get a
            // Markdown page generated (see ExportPluginGraphs) - an API call can just as easily
            // target some other entity that has none, so linking to it unconditionally produced
            // dead links in the "Reaches into" summary line.
            _entitiesWithOwnPage = new HashSet<string>(
                entityGraph.Owner?.Select(e => e.EntityName) ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            TotalSteps = allSteps.Count;
            SyncSteps = allSteps.Count(s => !s.Async);
            AsyncSteps = allSteps.Count(s => s.Async);
            SelfRecursionWarnings = allSteps.Where(s => s.HasSelfTrigger).ToList();
            AffectedEntities = allSteps
                .SelectMany(s => s.ApiCalls)
                .Select(c => c.EntityInfo?.LogicalName)
                .Where(name => !string.IsNullOrEmpty(name) && name != entityGraph.EntityName)
                .Distinct()
                .OrderBy(name => name)
                .Select(name => name!)
                .ToList();
            TotalApiCalls = allSteps.Sum(s => s.ApiCalls.Count);
            MessageCounts = entityGraph.Messages
                .Select(m => new MessageCount(m.Message, m.Stages.Values.Sum(steps => steps.Count)))
                .OrderByDescending(m => m.Count)
                .ToList();
        }

        public int TotalSteps { get; }
        public int SyncSteps { get; }
        public int AsyncSteps { get; }
        public int TotalApiCalls { get; }
        public List<MessageCount> MessageCounts { get; }
        public List<PluginStepInfo> SelfRecursionWarnings { get; }
        public List<string> AffectedEntities { get; }
        public bool HasSelfRecursionWarnings => SelfRecursionWarnings.Count > 0;
        public bool HasAffectedEntities => AffectedEntities.Count > 0;
        public string AffectedEntitiesMarkdown => string.Join(", ", AffectedEntities.Select(name =>
            _entitiesWithOwnPage.Contains(name) ? $"[{name}]({name}.md)" : name));
        public string MessageCountsMarkdown => string.Join(", ", MessageCounts.Select(m => $"{m.Message} ({m.Count})"));
    }

    public class MessageCount
    {
        public MessageCount(string message, int count)
        {
            Message = message;
            Count = count;
        }

        public string Message { get; }
        public int Count { get; }
    }
}
