using PentaWork.Xrm.PluginGraph.Templates;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    /// <summary>
    /// System-wide index page: a summary across every generated entity file, a table of contents,
    /// and an entity-level (not step-level) relationship diagram. The per-entity diagrams show
    /// every single step nested by message/stage, which grows with step count and stops being
    /// readable for any entity with more than a handful of steps - this instead aggregates
    /// relationships down to one node per entity, so the diagram size only depends on how many
    /// entities are actually involved in a cross-entity trigger, not how many steps they have.
    /// </summary>
    public class SystemOverview
    {
        public SystemOverview(EntityGraphList entityGraphList)
        {
            var entities = entityGraphList.OrderBy(e => e.EntityName, StringComparer.OrdinalIgnoreCase).ToList();

            Entities = entities.Select(e => new EntityOverviewRow(e)).ToList();
            TotalEntities = entities.Count;
            TotalSteps = entities.Sum(e => e.Summary.TotalSteps);
            TotalSyncSteps = entities.Sum(e => e.Summary.SyncSteps);
            TotalAsyncSteps = entities.Sum(e => e.Summary.AsyncSteps);
            TotalApiCalls = entities.Sum(e => e.Summary.TotalApiCalls);

            Edges = BuildEdges(entities);
            TriggerRelationshipCount = Edges.Count;

            var diagramEntityNames = Edges
                .SelectMany(e => new[] { e.FromEntity, e.ToEntity })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
            DiagramNodes = diagramEntityNames.Select(name => new EntityDiagramNode(name)).ToList();
        }

        /// <summary>
        /// Aggregates every step-to-step <see cref="TriggerLink"/> (already FilteringAttributes-aware,
        /// see <see cref="EntityGraphList.LinkTriggers"/>) down to one edge per (source entity,
        /// target entity) pair, collecting every distinct message that flows along it.
        /// </summary>
        private static List<EntityTriggerEdge> BuildEdges(List<EntityGraph> entities)
        {
            return entities
                .SelectMany(e => e.AllSteps)
                .SelectMany(step => step.TriggeredOtherSteps.Select(link => (From: step.PrimaryEntityName!, To: link.OtherStep.PrimaryEntityName!, link.Message)))
                .GroupBy(t => (t.From, t.To))
                .Select(g => new EntityTriggerEdge(g.Key.From, g.Key.To, g.Select(t => t.Message).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m, StringComparer.OrdinalIgnoreCase).ToList()))
                .OrderBy(e => e.FromEntity, StringComparer.OrdinalIgnoreCase).ThenBy(e => e.ToEntity, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public string ToMarkdown() => ScribanTemplateRenderer.Render("OverviewTemplate", new { Overview = this });

        public int TotalEntities { get; }
        public int TotalSteps { get; }
        public int TotalSyncSteps { get; }
        public int TotalAsyncSteps { get; }
        public int TotalApiCalls { get; }
        public int TriggerRelationshipCount { get; }
        public bool HasTriggerRelationships => TriggerRelationshipCount > 0;
        public List<EntityOverviewRow> Entities { get; }
        public List<EntityTriggerEdge> Edges { get; }
        public List<EntityDiagramNode> DiagramNodes { get; }
    }

    /// <summary>One row of the table of contents.</summary>
    public class EntityOverviewRow
    {
        public EntityOverviewRow(EntityGraph entityGraph)
        {
            EntityName = entityGraph.EntityName;
            FileName = $"{entityGraph.EntityName}.md";
            TotalSteps = entityGraph.Summary.TotalSteps;
            SyncSteps = entityGraph.Summary.SyncSteps;
            AsyncSteps = entityGraph.Summary.AsyncSteps;
            HasSelfRecursionWarnings = entityGraph.Summary.HasSelfRecursionWarnings;
        }

        public string EntityName { get; }
        public string FileName { get; }
        public int TotalSteps { get; }
        public int SyncSteps { get; }
        public int AsyncSteps { get; }
        public bool HasSelfRecursionWarnings { get; }
    }

    /// <summary>One node in the entity relationship diagram - only entities that actually take part in a cross-entity trigger, not every generated entity.</summary>
    public class EntityDiagramNode
    {
        public EntityDiagramNode(string entityName)
        {
            EntityName = entityName;
            MermaidId = MermaidIdFor(entityName);
        }

        public static string MermaidIdFor(string entityName) => $"e_{entityName}";

        public string EntityName { get; }
        public string MermaidId { get; }
    }

    /// <summary>One aggregated (source entity, target entity) trigger relationship, labeled with every distinct SDK message observed between them.</summary>
    public class EntityTriggerEdge
    {
        public EntityTriggerEdge(string fromEntity, string toEntity, List<string> messages)
        {
            FromEntity = fromEntity;
            ToEntity = toEntity;
            Messages = messages;
        }

        public string FromEntity { get; }
        public string ToEntity { get; }
        public List<string> Messages { get; }
        public string MessagesLabel => string.Join(", ", Messages);
        public bool IsSelfLoop => string.Equals(FromEntity, ToEntity, StringComparison.OrdinalIgnoreCase);
        public string FromMermaidId => EntityDiagramNode.MermaidIdFor(FromEntity);
        public string ToMermaidId => EntityDiagramNode.MermaidIdFor(ToEntity);
    }
}
