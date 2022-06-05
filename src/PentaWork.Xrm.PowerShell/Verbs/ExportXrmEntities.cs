using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;

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
        private readonly List<EntityMetadata> _fetchedMetaData = new List<EntityMetadata>();

        protected override void ProcessRecord()
        {
            var entityCollection = GetEntities();
            var metadata = GetMetadata(EntityName);
            var relevantAttributes = GetRelevantAttributes(metadata);
            WriteObject(GetEntityData(entityCollection, metadata, relevantAttributes));
        }

        private List<Entity> GetEntities()
        {
            var retrieveAction = new Func<int, int, EntityCollection>((pageSize, pageNumber) =>
            {
                var query = new QueryExpression
                {
                    EntityName = EntityName,
                    ColumnSet = ColumnSet,
                    Criteria = new FilterExpression(),
                    PageInfo = new PagingInfo { Count = pageSize, PageNumber = pageNumber }
                };
                FilterConditions.ForEach(c => query.Criteria.AddCondition(c));
                return Connection.RetrieveMultiple(query);
            });

            var entities = new List<Entity>();
            var response = retrieveAction(PageSize, PageNumber);

            entities.AddRange(response.Entities);
            if(GetAll && response.MoreRecords)
            {
                var pageNumber = PageNumber;
                while(response.MoreRecords)
                {
                    response = retrieveAction(PageSize, ++pageNumber);
                    entities.AddRange(response.Entities);
                }
            }

            return entities;
        }

        private EntityMetadata GetMetadata(string logicalName)
        {
            if(!_fetchedMetaData.Any(m => m.LogicalName == logicalName))
            {
                var request = new RetrieveEntityRequest
                {
                    LogicalName = logicalName,
                    EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                    RetrieveAsIfPublished = false
                };
                _fetchedMetaData.Add(((RetrieveEntityResponse)Connection.Execute(request)).EntityMetadata);
            }
            return _fetchedMetaData.Single(m => m.LogicalName == logicalName);
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
            foreach(var entity in entities)
            {
                var entityInfo = new EntityInfo
                {
                    Id = (Guid)entity.Attributes[relevantAttributes.Single(a => a.IsPrimaryId == true).LogicalName],
                    Name = entity.Attributes.Contains(metadata.PrimaryNameAttribute) ? entity.Attributes[metadata.PrimaryNameAttribute].ToString() : string.Empty
                };

                var attributeInfos = new List<AttributeInfo>();
                foreach(var attr in entity.Attributes)
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
                entityInfo.Attributes = attributeInfos.ToArray();
                entityInfos.Add(entityInfo);
            }
            exportInfo.Entities = entityInfos.ToArray();
            return exportInfo;
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
                    var metadata = GetMetadata(entityRef.LogicalName);

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
