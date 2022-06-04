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
            var user = TryRetrieve(FallbackOwner.LogicalName, FallbackOwner.Id, new ColumnSet("isdisabled"));
            if (user == null) throw new Exception("Could not find fallback owner in connected system!");
            if ((bool?)user.Attributes["isdisabled"] == true) throw new Exception("Given fallback user is disabled in connected system!");
        }

        private Entity TryRetrieve(string logicalName, Guid id, ColumnSet columns)
        {
            Entity entity = null;
            try
            {
                entity = Connection.Retrieve(logicalName, id, columns);
            }
            catch(FaultException) { /*No entity found*/ }
            return entity;
        }

        private void ImportEntities()
        {
            int created = 0;
            int updated = 0;
            Console.WriteLine($"Importing entities ...");
            foreach(var entity in EntityData.Entities)
            {
                Entity systemEntity = null;
                if (MapByName)
                {
                    var foundEntities = GetEntitiesByName(EntityData.EntityName, EntityData.PrimaryNameField, entity.Name);
                    if (foundEntities.Count > 0) Console.WriteLine($"Found multiple matches for '{entity.Name}' ...");
                    if (foundEntities.Count > 0 && !TakeFirst) { Console.WriteLine("Skipping ..."); continue; }

                    systemEntity = foundEntities.FirstOrDefault();
                }
                else systemEntity = TryRetrieve(EntityData.EntityName, entity.Id, new ColumnSet());

                var isUpdate = true;
                if (systemEntity == null && UpdateOnly) { Console.WriteLine("No entity found. Update Only! Skipping ..."); continue; }
                else if (systemEntity == null) { systemEntity = new Entity(EntityData.EntityName); isUpdate = false; }

                foreach(var attr in entity.Attributes)
                {
                    systemEntity[attr.Name] = DeserializeValue(attr.Type, attr.Value);
                }

                var owner = (EntityReference)systemEntity["ownerid"];
                var systemOwner = GetEntitiesByName(owner.LogicalName, owner.LogicalName == "systemuser" ? "fullname" : "name", owner.Name);
                if(systemOwner.Count == 0) { Console.WriteLine($"Owner '{owner.Name}' not found! Using fallback user ..."); systemEntity["ownerid"] = FallbackOwner; }
                else systemEntity["ownerid"] = systemOwner.First().ToEntityReference();

                if (isUpdate) { Connection.Update(systemEntity); updated++; }
                else { Connection.Create(systemEntity); created++; }
            }
            Console.WriteLine($"Created: {created} Updated: {updated}");
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

        private object DeserializeValue(AttributeTypeCode? type, string value)
        {
            object deserializedValue = null;
            switch (type)
            {
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.Picklist:
                    deserializedValue = new OptionSetValue(int.Parse(value));
                    break;
                case AttributeTypeCode.DateTime:
                    deserializedValue = DateTime.Parse(value);
                    break;
                case AttributeTypeCode.Boolean:
                    deserializedValue = bool.Parse(value);
                    break;
                case AttributeTypeCode.Money:
                case AttributeTypeCode.Decimal:
                    deserializedValue = decimal.Parse(value);
                    break;
                case AttributeTypeCode.Double:
                    deserializedValue = double.Parse(value);
                    break;
                case AttributeTypeCode.Integer:
                    deserializedValue = int.Parse(value);
                    break;
                case AttributeTypeCode.BigInt:
                    deserializedValue = BigInteger.Parse(value);
                    break;
                case AttributeTypeCode.Uniqueidentifier:
                    deserializedValue = new Guid(value);
                    break;
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                    var entityRef = value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                    deserializedValue = new EntityReference(entityRef[0], new Guid(entityRef[1])) { Name = entityRef[2] };
                    break;
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.EntityName:
                    deserializedValue = value;
                    break;
                default:
                    throw new Exception($"{type}");
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
        /// <para type="description">Set, if no new entities should be created. Default is upsert (update and create)</para>
        /// </summary>
        [Parameter]
        public SwitchParameter UpdateOnly { get; set; }

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
    }
}
