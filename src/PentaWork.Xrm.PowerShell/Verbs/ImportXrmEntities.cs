using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Numerics;
using PentaWork.Xrm.PowerShell.Common;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Imports given entities to the given xrm system.</para>
    /// <para type="description">
    /// This function imports a given set of entities to the connected xrm system.
    /// The result of the <c>Export-XrmEntities</c> can be piped into this function.
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
            var fallbackOwner = Connection.TryRetrieve(FallbackOwner.LogicalName, FallbackOwner.Id, new ColumnSet(true));
            if (fallbackOwner == null) throw new Exception("Could not find fallback owner in connected system!");
            if (fallbackOwner.Attributes.Contains("isdisabled") && (bool?)fallbackOwner.Attributes["isdisabled"] == true) throw new Exception("Given fallback user is disabled in connected system!");

            // Set the fallback owner name for logging
            FallbackOwner.Name = fallbackOwner.Contains("fullname") ? fallbackOwner.Attributes["fullname"].ToString() : fallbackOwner.Id.ToString();
            FallbackOwner.Name = FallbackOwner.Name == string.Empty && fallbackOwner.Contains("name") ? fallbackOwner.Attributes["name"].ToString() : FallbackOwner.Name;

            int created = 0;
            int updated = 0;
            int processed = 0;
            var metadata = Connection.GetMetadata(EntityData.EntityName);

            foreach (var entityInfo in EntityData.Entities)
            {
                WriteProgress(new ProgressRecord(0, "Importing", $"Importing entity '{entityInfo.Name}' ...") { PercentComplete = 100 * processed++ / EntityData.Entities.Length });
                try
                {
                    var matchingSystemEntities = Connection.GetMatchingEntities(EntityData.EntityName, entityInfo.Id, entityInfo.Name, MapByName ? EntityData.PrimaryNameField : null);
                    var systemEntity = matchingSystemEntities.FirstOrDefault();
                    var isUpdate = systemEntity != null;

                    if (CreateOnly.Contains(entityInfo.Name) && isUpdate) { WriteVerbose($"Skipping update of '{entityInfo.Name}' - Create Only ..."); continue; }
                    if (matchingSystemEntities.Count > 1) WriteVerbose($"Found multiple matches for '{entityInfo.Name}' ...");
                    if (matchingSystemEntities.Count > 1 && !TakeFirst) { WriteVerbose("Skipping ..."); continue; }

                    systemEntity = systemEntity ?? new Entity(EntityData.EntityName);
                    SetAttributes(systemEntity, entityInfo.Attributes, isUpdate);
                    var ownerRef = SetOwner(systemEntity, metadata);
                    ExecuteChange(systemEntity, isUpdate);

                    WriteVerbose($"{(isUpdate ? "UPDATED" : "CREATED")}: {entityInfo.Name} {(ownerRef != null ? $"[Owner: {ownerRef.Name}]" : "")}");
                    created += isUpdate ? 0 : 1;
                    updated += isUpdate ? 1 : 0;
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "ImportError", ErrorCategory.WriteError, null));
                }
            }
            WriteProgress(new ProgressRecord(0, "Importing", "Done!") { RecordType = ProgressRecordType.Completed });
            WriteVerbose($"Created: {created} Updated: {updated}");
        }

        private void SetAttributes(Entity systemEntity, AttributeInfo[] attributes, bool isUpdate)
        {
            foreach (var attr in attributes)
            {
                // Skip Id Attribute on Update
                if (isUpdate && attr.Name == EntityData.PrimaryIdField) continue;
                // Skip Id Attribute, if mapped by name. Otherwise it could be a problem for certain entities like userqueries for example.
                // If a user query gets created, and reassigned, the module wont find the view and would try to 
                // create a entity with an entity already present in the system.
                if (MapByName && attr.Name == EntityData.PrimaryIdField) continue; 
                systemEntity[attr.Name] = DeserializeValue(attr);
            }
        }

        private EntityReference SetOwner(Entity entity, EntityMetadata metadata)
        {
            // Some entities like themes and currencies dont have an owner!
            if (!metadata.Attributes.Any(a => a.LogicalName == "ownerid")) return null;

            var owner = FallbackOwner;
            // New entities dont have an ownerid set, if the importing data is not containing one.
            if (entity.Contains("ownerid"))
            {
                owner = (EntityReference)entity["ownerid"];
                var systemOwner = Connection.GetEntitiesByName(owner.LogicalName, owner.LogicalName == "systemuser" ? "fullname" : "name", owner.Name);

                // Check, if the given owner exists in the target system
                if (systemOwner.Count != 0) owner = systemOwner.First().ToEntityReference();
            }

            entity["ownerid"] = owner;
            return owner;
        }

        private void ExecuteChange(Entity entity, bool isUpdate)
        {
            var orignalCaller = Connection.CallerId;
            if (ImpersonateOwner && entity.Attributes.Contains("ownerid")) Connection.CallerId = ((EntityReference)entity["ownerid"]).Id;

            if (isUpdate) Connection.Update(entity);
            else Connection.Create(entity);

            if (ImpersonateOwner) Connection.CallerId = orignalCaller;
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
                        var entities = Connection.GetEntitiesByName(entityRef[0], entityRef[3], entityRef[2]);
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
        /// <para type="description">Set to true, if the call for creates and updates should impersonate the target owner
        /// (either the fallback owner, the owner of the existing entity or the owner given in the entity data to import).</para>
        /// </summary>
        [Parameter]
        public SwitchParameter ImpersonateOwner { get; set; }

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
