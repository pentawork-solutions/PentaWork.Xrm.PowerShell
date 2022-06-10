using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.PowerShell.Common;
using Microsoft.Xrm.Sdk.Query;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    #region DTOs
    /// <summary>
    /// <para type="synopsis">Object to hold information for additional relation map conditions.</para>
    /// <para type="description">
    /// Holds some meta data for additional relation map conditions.
    /// </para>
    /// </summary>
    public class RelationCondition
    {
        public string SchemaName { get; set; }
        public string TargetField { get; set; }
        public string SourceField { get; set; }
    }
    #endregion

    /// <summary>
    /// <para type="synopsis">Imports given N:M relations to the given xrm system.</para>
    /// <para type="description">
    /// This function imports a given set of given N:M relations to the connected xrm system.
    /// The result of the <c>Export-XrmEntities</c> can be piped into this function.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -InteractiveMode</para>
    /// <para>Export-XrmEntities -EntityName team -Connection $conn -Relations @("teamroles_association") | Import-XrmRelations -Connection $conn</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "XrmRelations")]
    public class ImportXrmRelations : PSCmdlet
    {
        private List<Entity> _relationEntities = new List<Entity>();

        protected override void ProcessRecord()
        {
            int processedEntities = 0;
            foreach (var entityInfo in EntityData.Entities)
            {
                if (entityInfo.Relations.Any())
                {
                    WriteProgress(new ProgressRecord(0, "Importing", $"Importing relations for '{entityInfo.Name}' ...") { PercentComplete = 100 * processedEntities++ / EntityData.Entities.Length });

                    var matchingSystemEntities = Connection.GetMatchingEntities(EntityData.EntityName, entityInfo.Id, entityInfo.Name, MapEntitiesByName ? EntityData.PrimaryNameField : null);
                    if (matchingSystemEntities.Count > 1) WriteWarning($"Multiple entities for '{entityInfo.Name}' found ...");
                    if (matchingSystemEntities.Count == 0)
                    {
                        WriteWarning($"Entity '{entityInfo.Name}' not found!");
                        continue;
                    }

                    int processedRelations = 0;
                    var relatingEntity = matchingSystemEntities.First();
                    foreach (var relationInfo in entityInfo.Relations)
                    {
                        var relatedEntityReferences = new EntityReferenceCollection();
                        foreach (var relation in relationInfo.Entities)
                        {
                            WriteProgress(new ProgressRecord(1, "Importing", $"Importing relations for schema '{relationInfo.SchemaName}' ...") { PercentComplete = 100 * processedRelations++ / entityInfo.Relations.Sum(r => r.Entities.Length) });

                            var relatedEntity = Connection.TryRetrieve(relationInfo.ToLogicalName, relation.Id, new ColumnSet());
                            if (MapRelatedEntitiesByName)
                            {
                                var query = new QueryExpression
                                {
                                    EntityName = relationInfo.ToLogicalName,
                                    ColumnSet = new ColumnSet(true)
                                };
                                query.Criteria.AddCondition(relationInfo.ToPrimaryNameAttribute, ConditionOperator.Equal, relation.Name);
                                AddRelationConditions(query, relatingEntity, relationInfo.SchemaName);

                                var matches = Connection.Query(query);
                                if (matches.Count > 1) WriteWarning($"Multiple related entities for '{relation.Name}' found ...");
                                relatedEntity = matches.FirstOrDefault();
                            }
                            if (relatedEntity == null)
                            {
                                WriteWarning($"Related entity '{relation.Name}' not found!");
                                continue;
                            }
                            relatedEntityReferences.Add(relatedEntity.ToEntityReference());
                        }
                        WriteProgress(new ProgressRecord(1, "Importing", "Done!") { RecordType = ProgressRecordType.Completed });

                        Connection.Disassociate(relatingEntity.LogicalName, relatingEntity.Id, new Relationship(relationInfo.SchemaName), relatedEntityReferences);
                        Connection.Associate(relatingEntity.LogicalName, relatingEntity.Id, new Relationship(relationInfo.SchemaName), relatedEntityReferences);
                    }
                }
            }
            WriteProgress(new ProgressRecord(0, "Importing", "Done!") { RecordType = ProgressRecordType.Completed });
        }

        private void AddRelationConditions(QueryExpression query, Entity relatingEntity, string schemaName)
        {
            foreach (var condition in RelationConditions.Where(r => r.SchemaName == schemaName))
            {
                if (!relatingEntity.Contains(condition.SourceField)) continue;

                var value = relatingEntity[condition.SourceField];
                if (value is EntityReference) value = ((EntityReference)value).Id;

                query.Criteria.AddCondition(condition.TargetField, ConditionOperator.Equal, value);
            }
        }

        /// <summary>
        /// <para type="description">The connection to the XRM Organization (Get-CrmConnection).</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public CrmServiceClient Connection { get; set; }

        /// <summary>
        /// <para type="description">The entities with sharings to import.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public EntityData EntityData { get; set; }

        /// <summary>
        /// <para type="description">Set, if the entities should get mapped by name instead of the id.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter MapEntitiesByName { get; set; }

        /// <summary>
        /// <para type="description">Set, if the entities to relate should get mapped by name instead of the id.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter MapRelatedEntitiesByName { get; set; }

        /// <summary>
        /// <para type="description">Additional mapping conditions to retrieve matching entities, if using <c>MapRelatedEntitiesByName</c>.
        /// This can be especially helpful for syncing security roles where the business unit has to be the same as the business unit of the regarding team or user.</para>
        /// </summary>
        [Parameter]
        public List<RelationCondition> RelationConditions { get; set; } = new List<RelationCondition>();
    }
}
