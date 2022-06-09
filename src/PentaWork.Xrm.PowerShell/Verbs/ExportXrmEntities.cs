using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using Microsoft.Crm.Sdk.Messages;
using PentaWork.Xrm.PowerShell.Common;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    #region DTOs
    /// <summary>
    /// <para type="synopsis">Object to hold the entity data.</para>
    /// <para type="description">
    /// Holds some meta data for the retrieved entity type and the export process.
    /// </para>
    /// </summary>
    public class EntityData
    {
        public string EntityName { get; set; }
        public string PrimaryIdField { get; set; }
        public string PrimaryNameField { get; set; }
        public string ExportedFrom { get; set; }
        public DateTime ExportedOn { get; set; }
        public EntityInfo[] Entities { get; set; }
    }

    /// <summary>
    /// <para type="synopsis">Object to hold the single entity information.</para>
    /// <para type="description">
    /// Holds all writable attribute values, the id and name of the entity.
    /// </para>
    /// </summary>
    public class EntityInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public AttributeInfo[] Attributes { get; set; }
        public RelationInfo[] Relations { get; set; }
        public ShareInfo[] Sharings { get; set; }
    }

    /// <summary>
    /// <para type="synopsis">Object to hold the attribute information.</para>
    /// <para type="description">
    /// Holds some meta data for the retrieved attribute and the value itself.
    /// </para>
    /// </summary>
    public class AttributeInfo
    {
        public string Name { get; set; }
        public AttributeTypeCode? Type { get; set; }
        public string Value { get; set; }
        public bool ValidForCreate { get; set; }
        public bool ValidForUpdate { get; set; }
    }

    /// <summary>
    /// <para type="synopsis">Object to hold the relation information.</para>
    /// <para type="description">
    /// Holds some meta data for the retrieved relation.
    /// </para>
    /// </summary>
    public class RelationInfo
    {
        public string SchemaName { get; set; }
        public string IntersectEntityName { get; set; }
        public string ToLogicalName { get; set; }
        public string ToPrimaryNameAttribute { get; set; }
        public RelatedEntityInfo[] Entities { get; set; }
    }

    /// <summary>
    /// <para type="synopsis">Object to hold an entity reference.</para>
    /// </summary>
    public class RelatedEntityInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// <para type="synopsis">Object to hold a asharing information.</para>
    /// </summary>
    public class ShareInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LogicalName { get; set; }
        public AccessRights AccessMask { get; set; }
    }
    #endregion

    /// <summary>
    /// <para type="synopsis">Lists all entities of the given entity type.</para>
    /// <para type="description">
    /// This function lists all entities of a given entity type. The result can be saved into json.
    /// The results of the <c>Get-XrmSolutions</c> function can be piped into this one.
    /// The <c>EntityData</c> can be piped into the <c>Import-XrmEntities</c> function.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>Get-CrmConnection -InteractiveMode | Get-XrmEntities -EntityName userquery | ConvertTo-Json -depth 4 | Out-File "userquery.json"</para>
    /// </example>
    [OutputType(typeof(EntityData))]
    [Cmdlet(VerbsData.Export, "XrmEntities")]
    public class ExportXrmEntities : PSCmdlet
    {
        private readonly ConsoleLogger _logger = new ConsoleLogger();
        private readonly List<EntityMetadata> _fetchedMetaData = new List<EntityMetadata>();

        protected override void ProcessRecord()
        {
            var entityCollection = GetEntities();
            var metadata = Connection.GetMetadata(EntityName);
            var relevantAttributes = GetRelevantAttributes(metadata);
            WriteObject(GetEntityData(entityCollection, metadata, relevantAttributes));
        }

        private List<Entity> GetEntities()
        {
            var query = new QueryExpression
            {
                EntityName = EntityName,
                ColumnSet = ColumnSet,
                PageInfo = new PagingInfo { Count = PageSize, PageNumber = PageNumber }
            };
            FilterConditions.ForEach(c => query.Criteria.AddCondition(c));
            return Connection.Query(query, GetAll);
        }

        private List<AttributeMetadata> GetRelevantAttributes(EntityMetadata metadata)
        {
            var filter = new Func<AttributeMetadata, bool>((attr) =>
            {
                bool relevant = false;
                if (ValidForCreate) relevant = attr.IsValidForCreate == true;
                if (ValidForUpdate) relevant = relevant || attr.IsValidForUpdate == true;
                if (!ValidForCreate && !ValidForUpdate) relevant = attr.IsValidForUpdate == true || attr.IsValidForCreate == true;
                return relevant && !IgnoreColumns.Contains(attr.LogicalName);
            });
            return metadata.Attributes.Where(a => filter(a)).ToList();
        }

        private EntityData GetEntityData(List<Entity> entities, EntityMetadata metadata, List<AttributeMetadata> relevantAttributes)
        {
            var exportInfo = new EntityData
            {
                EntityName = EntityName,
                PrimaryIdField = metadata.PrimaryIdAttribute,
                PrimaryNameField = metadata.PrimaryNameAttribute,
                ExportedFrom = Connection.ConnectedOrgUniqueName,
                ExportedOn = DateTime.UtcNow
            };

            var entityInfos = new List<EntityInfo>();
            foreach (var entity in entities)
            {
                var entityInfo = new EntityInfo
                {
                    Id = metadata.PrimaryIdAttribute != null && entity.Attributes.Contains(metadata.PrimaryIdAttribute) ? (Guid)entity.Attributes[metadata.PrimaryIdAttribute] : Guid.Empty,
                    Name = metadata.PrimaryNameAttribute != null && entity.Attributes.Contains(metadata.PrimaryNameAttribute) ? entity.Attributes[metadata.PrimaryNameAttribute].ToString() : string.Empty
                };
                entityInfo.Attributes = GetAttributes(entity, relevantAttributes);
                entityInfo.Sharings = GetSharings(entity);
                entityInfo.Relations = GetRelations(entity, metadata);

                entityInfos.Add(entityInfo);
            }
            exportInfo.Entities = entityInfos.ToArray();
            return exportInfo;
        }

        private AttributeInfo[] GetAttributes(Entity entity, List<AttributeMetadata> relevantAttributes)
        {
            var attributeInfos = new List<AttributeInfo>();
            foreach (var attr in entity.Attributes)
            {
                var attrMetadata = relevantAttributes.SingleOrDefault(ra => ra.LogicalName == attr.Key);
                if (attrMetadata == null) continue;

                attributeInfos.Add(new AttributeInfo
                {
                    Name = attr.Key,
                    ValidForCreate = attrMetadata.IsValidForCreate == true,
                    ValidForUpdate = attrMetadata.IsValidForUpdate == true,
                    Type = attrMetadata.AttributeType,
                    Value = SerializeValue(attrMetadata.AttributeType, attr.Value)
                }); ;
            }
            return attributeInfos.ToArray();
        }

        private ShareInfo[] GetSharings(Entity entity)
        {
            if(!Sharings) return new ShareInfo[0];

            var shares = new List<ShareInfo>();
            var request = new RetrieveSharedPrincipalsAndAccessRequest { Target = entity.ToEntityReference() };
            var response = (RetrieveSharedPrincipalsAndAccessResponse) Connection.Execute(request);
            foreach(var share in response.PrincipalAccesses)
            {
                var primaryNameField = share.Principal.LogicalName == "systemuser"
                    ? "fullname"
                    : "name"; // Teams
                var principal = Connection.Retrieve(share.Principal.LogicalName, share.Principal.Id, new ColumnSet(primaryNameField));
                var principalName = principal.Contains(primaryNameField) ? principal[primaryNameField].ToString() : string.Empty;

                var shareInfo = new ShareInfo
                {
                    Id = share.Principal.Id,
                    Name = principalName,
                    LogicalName = share.Principal.LogicalName,
                    AccessMask = share.AccessMask
                };

                shares.Add(shareInfo);
            }
            return shares.ToArray();
        }

        private RelationInfo[] GetRelations(Entity entity, EntityMetadata entityMetadata)
        { 
            if(Relations.Count == 0) return new RelationInfo[0];

            var relations = new List<RelationInfo>();
            foreach(var relation in Relations)
            {
                var schemaDefinition = entityMetadata.ManyToManyRelationships.SingleOrDefault(r => r.SchemaName == relation);
                if (schemaDefinition == null)
                {
                    _logger.Warn($"No relation definition matching the schema name '{relation}' was found!");
                    continue;
                }

                var fromEntityIdField = schemaDefinition.Entity1LogicalName == entity.LogicalName ? schemaDefinition.Entity1IntersectAttribute : schemaDefinition.Entity2IntersectAttribute;
                var toEntityLogicalName = schemaDefinition.Entity1LogicalName == entity.LogicalName ? schemaDefinition.Entity2LogicalName : schemaDefinition.Entity1LogicalName;
                var toEntityIdField = schemaDefinition.Entity1LogicalName == entity.LogicalName ? schemaDefinition.Entity2IntersectAttribute : schemaDefinition.Entity1IntersectAttribute;
                var toEntityMetaData = Connection.GetMetadata(toEntityLogicalName);

                var relationQuery = new QueryExpression
                {
                    EntityName = schemaDefinition.IntersectEntityName,
                    ColumnSet = new ColumnSet(true)
                };
                relationQuery.Criteria.AddCondition(fromEntityIdField, ConditionOperator.Equal, entity.Id);
                var relationEntities = Connection.Query(relationQuery, true);
                if (relationEntities.Count == 0) continue;

                var relatedEntitiesQuery = new QueryExpression
                {
                    EntityName = toEntityLogicalName,
                    ColumnSet = new ColumnSet(toEntityMetaData.PrimaryNameAttribute)
                };
                var condition = new ConditionExpression(toEntityMetaData.PrimaryIdAttribute, ConditionOperator.In, relationEntities.Select(r => (Guid)r[toEntityIdField]).ToArray());
                relatedEntitiesQuery.Criteria.AddCondition(condition);

                var relatedEntities = Connection.Query(relatedEntitiesQuery, true);
                var relationInfo = new RelationInfo
                {
                    SchemaName = schemaDefinition.SchemaName,
                    IntersectEntityName = schemaDefinition.IntersectEntityName,
                    ToLogicalName = toEntityMetaData.LogicalName,
                    ToPrimaryNameAttribute = toEntityMetaData.PrimaryNameAttribute,
                    Entities = relatedEntities.Select(r => new RelatedEntityInfo
                    { 
                        Id = r.Id, 
                        Name = r.Contains(toEntityMetaData.PrimaryNameAttribute) ? r[toEntityMetaData.PrimaryNameAttribute].ToString() : string.Empty
                    }).ToArray()
                };
                relations.Add(relationInfo);
            }
            return relations.ToArray();
        }

        private string SerializeValue(AttributeTypeCode? type, object value)
        {
            var serializedValue = string.Empty;
            switch (type)
            {
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.Picklist:
                    var optionSetValue = (OptionSetValue)value;
                    serializedValue = optionSetValue.Value.ToString();
                    break;
                case AttributeTypeCode.DateTime:
                case AttributeTypeCode.Boolean:
                case AttributeTypeCode.Money:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.BigInt:
                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Uniqueidentifier:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.EntityName:
                    serializedValue = value.ToString();
                    break;
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                    var entityRef = (EntityReference)value;
                    if (!_fetchedMetaData.Any(m => m.LogicalName == entityRef.LogicalName))
                    {
                        _fetchedMetaData.Add(Connection.GetMetadata(entityRef.LogicalName));
                    }

                    var metadata = _fetchedMetaData.Single(m => m.LogicalName == entityRef.LogicalName);
                    serializedValue = $"{entityRef.LogicalName};{entityRef.Id};{entityRef.Name};{metadata.PrimaryNameAttribute}";
                    break;
                default:
                    throw new Exception($"{type}");
            }
            return serializedValue;
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
        /// <para type="description">The entity type to list.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string EntityName { get; set; }

        /// <summary>
        /// <para type="description">The columns to retrieve. Default is all columns.</para>
        /// </summary>
        [Parameter]
        public ColumnSet ColumnSet { get; set; } = new ColumnSet(true);

        /// <summary>
        /// <para type="description">Filter conditions to filter the retrieved entities.</para>
        /// </summary>
        [Parameter]
        public List<ConditionExpression> FilterConditions { get; set; } = new List<ConditionExpression>();

        /// <summary>
        /// <para type="description">List of columns which should get filtered out in the result set.</para>
        /// </summary>
        [Parameter]
        public List<string> IgnoreColumns { get; set; } = new List<string>();

        /// <summary>
        /// <para type="description">List of relation schemas to export.</para>
        /// </summary>
        [Parameter]
        public List<string> Relations { get; set; } = new List<string>();

        /// <summary>
        /// <para type="description">The page size to show. Default is 500.</para>
        /// </summary>
        [Parameter]
        public int PageSize { get; set; } = 500;

        /// <summary>
        /// <para type="description">The page to retrieve. Default is the first page.</para>
        /// </summary>
        [Parameter]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// <para type="description">Set to true, if the retrieve of all entites in the system is desired. This will do multiple calls to the xrm system according to the page size.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter GetAll { get; set; }

        /// <summary>
        /// <para type="description">Set to true, to export the sharings for the exported entities.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Sharings { get; set; }

        /// <summary>
        /// <para type="description">Only get attributes which are valid for create. Default is false.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter ValidForCreate { get; set; }

        /// <summary>
        /// <para type="description">Only get attributes which are valid for update. Default is false.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter ValidForUpdate { get; set; }
    }
}
