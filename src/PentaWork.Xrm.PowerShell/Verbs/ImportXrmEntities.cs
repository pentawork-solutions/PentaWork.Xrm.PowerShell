using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Numerics;
using System.ServiceModel;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Imports given entities to the given xrm system.</para>
    /// <para type="description">
    /// This function imports a given set of entities to the connected xrm system.
    /// The result of the <c>Get-XrmEntities</c> can be piped into this function.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -InteractiveMode</para>
    /// <para>$fallbackOwner = [Microsoft.Xrm.Sdk.EntityReference]::new("systemuser", [System.Guid]::new("D78327BB-A5F2-EA11-A86A-005056B5F580"))</para>
    /// <para>Get-Content "userquery.json" | ConvertFrom-Json | Import-XrmEntities -Connection $conn -FallbackOwner $fallbackOwner</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "XrmEntities")]
    public class ImportXrmEntities : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            CheckFallbackOwner();
            ImportEntities();
        }

        private void CheckFallbackOwner()
        {
            var owner = TryRetrieve(FallbackOwner.LogicalName, FallbackOwner.Id, new ColumnSet(true));
            if (owner == null) throw new Exception("Could not find fallback owner in connected system!");
            if (owner.Attributes.Contains("isdisabled") && (bool?)owner.Attributes["isdisabled"] == true) throw new Exception("Given fallback user is disabled in connected system!");
        }

        private void ImportEntities()
        {
            int created = 0;
            int updated = 0;
            Console.WriteLine($"Importing entities ...");
            foreach(var entityInfo in EntityData.Entities)
            {
                try
                {
                    var matchingSystemEntities = GetMatchingSystemEntities(entityInfo);
                    var systemEntity = matchingSystemEntities.FirstOrDefault();
                    var isUpdate = systemEntity != null;

                    if (CreateOnly.Contains(entityInfo.Name) && isUpdate) Console.WriteLine($"Skipping update of '{entityInfo.Name}' - Create Only ...");
                    if (matchingSystemEntities.Count > 1) Console.WriteLine($"Found multiple matches for '{entityInfo.Name}' ...");
                    if (matchingSystemEntities.Count > 1 && !TakeFirst) { Console.WriteLine("Skipping ..."); continue; }

                    systemEntity = systemEntity ?? new Entity(EntityData.EntityName);
                    SetAttributes(systemEntity, entityInfo.Attributes, isUpdate);
                    SetOwner(systemEntity);

                    if (isUpdate)
                    {
                        Connection.Update(systemEntity);
                        updated++;
                    }
                    else
                    {
                        Connection.Create(systemEntity);
                        created++;
                    }
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }                
            }
            Console.WriteLine($"Created: {created} Updated: {updated}");
        }

        private List<Entity> GetMatchingSystemEntities(EntityInfo entityInfo)
        {
            List<Entity> entities;
            if (MapByName)
            {
                entities = GetEntitiesByName(EntityData.EntityName, EntityData.PrimaryNameField, entityInfo.Name);
            }
            else
            {
                var entity = TryRetrieve(EntityData.EntityName, entityInfo.Id, new ColumnSet());
                entities = entity != null
                    ? new List<Entity> { entity }
                    : new List<Entity>();
            }
            return entities;
        }

        private void SetAttributes(Entity systemEntity, AttributeInfo[] attributes, bool isUpdate)
        {
            foreach (var attr in attributes)
            {
                if (isUpdate && attr.Name == EntityData.PrimaryIdField) continue; // Skip Id Attribute on Update
                systemEntity[attr.Name] = DeserializeValue(attr);
            }
        }

        private void SetOwner(Entity systemEntity)
        {
            // Some entities like themes and currencies dont have an owner!
            if (!systemEntity.Attributes.Contains("ownerid")) return;

            var owner = (EntityReference)systemEntity["ownerid"];
            var systemOwner = GetEntitiesByName(owner.LogicalName, owner.LogicalName == "systemuser" ? "fullname" : "name", owner.Name);
            if (systemOwner.Count == 0) systemEntity["ownerid"] = FallbackOwner;
            else systemEntity["ownerid"] = systemOwner.First().ToEntityReference();
        }

        private Entity TryRetrieve(string logicalName, Guid id, ColumnSet columns)
        {
            Entity entity = null;
            try
            {
                entity = Connection.Retrieve(logicalName, id, columns);
            }
            catch (FaultException) { /*No entity found*/ }
            return entity;
        }

        private List<Entity> GetEntitiesByName(string entityLogicalName, string nameField, string entityName)
        {
            var query = new QueryExpression
            {
                EntityName = entityLogicalName,
                ColumnSet = new ColumnSet(nameField),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(nameField, ConditionOperator.Equal, entityName);
            return Connection.RetrieveMultiple(query).Entities.ToList();
        }

        private object DeserializeValue(AttributeInfo attr)
        {
            object deserializedValue = null;
            switch (attr.Type)
            {
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.Picklist:
                    deserializedValue = new OptionSetValue(int.Parse(attr.Value));
                    break;
                case AttributeTypeCode.DateTime:
                    deserializedValue = DateTime.Parse(attr.Value);
                    break;
                case AttributeTypeCode.Boolean:
                    deserializedValue = bool.Parse(attr.Value);
                    break;
                case AttributeTypeCode.Money:
                case AttributeTypeCode.Decimal:
                    deserializedValue = decimal.Parse(attr.Value);
                    break;
                case AttributeTypeCode.Double:
                    deserializedValue = double.Parse(attr.Value);
                    break;
                case AttributeTypeCode.Integer:
                    deserializedValue = int.Parse(attr.Value);
                    break;
                case AttributeTypeCode.BigInt:
                    deserializedValue = BigInteger.Parse(attr.Value);
                    break;
                case AttributeTypeCode.Uniqueidentifier:
                    deserializedValue = new Guid(attr.Value);
                    break;
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                    var entityRef = attr.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    deserializedValue = new EntityReference(entityRef[0], new Guid(entityRef[1])) { Name = entityRef[2] };
                    if (MapByNameLookups.Contains(attr.Name))
                    {
                        var entities = GetEntitiesByName(entityRef[0], entityRef[3], entityRef[2]);
                        if (entities.FirstOrDefault() != null) deserializedValue = entities.First().ToEntityReference();
                        else throw new Exception($"No matching '{entityRef[0]}' for name '{entityRef[2]}' found!");
                    }
                    break;
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.EntityName:
                    deserializedValue = attr.Value;
                    break;
                default:
                    throw new Exception($"{attr.Type}");
            }
            return deserializedValue;
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
        /// <para type="description">The entities to import.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public EntityData EntityData { get; set; }

        /// <summary>
        /// <para type="description">The fallback user id to use for new entities, if the given one is not found (mapped by name).</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public EntityReference FallbackOwner { get; set; }

        /// <summary>
        /// <para type="description">Set, if the mapping for updates should be done by entity name instead of the id.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter MapByName { get; set; }

        /// <summary>
        /// <para type="description">Set, if the first found matching entity should be used for updates (only relevant with the <c>MapByName</c> option). Otherwise it will be skipped.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter TakeFirst { get; set; }

        /// <summary>
        /// <para type="description">The values for the listed lookup fields will be matches by its primary name value.</para>
        /// </summary>
        [Parameter]
        public List<string> MapByNameLookups { get; set; } = new List<string>();

        /// <summary>
        /// <para type="description">The entity names listed will only be created, but not updated, if found.</para>
        /// </summary>
        [Parameter]
        public List<string> CreateOnly { get; set; } = new List<string>();
    }
}
