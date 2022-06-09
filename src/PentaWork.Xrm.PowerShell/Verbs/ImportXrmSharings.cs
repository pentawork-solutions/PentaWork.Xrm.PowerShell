using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.PowerShell.Common;
using Microsoft.Crm.Sdk.Messages;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Imports given sharings to the given xrm system.</para>
    /// <para type="description">
    /// This function imports a given set of sharings to the connected xrm system.
    /// The result of the <c>Export-XrmEntities</c> can be piped into this function.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -InteractiveMode</para>
    /// <para>Export-XrmEntities -EntityName userquery -Connection $conn | Import-XrmSharings -Connection $conn</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "XrmSharings")]
    public class ImportXrmSharings : PSCmdlet
    {
        private List<Entity> _principals = new List<Entity>();

        protected override void ProcessRecord()
        {
            int processedEntities = 0;
            foreach(var entityInfo in EntityData.Entities)
            {
                if(entityInfo.Sharings.Any())
                {
                    WriteProgress(new ProgressRecord(0, "Importing", $"Importing shares for '{entityInfo.Name}' ...") { PercentComplete = 100 * processedEntities / EntityData.Entities.Length });
                                        
                    var matchingSystemEntities = Connection.GetMatchingEntities(EntityData.EntityName, entityInfo.Id, entityInfo.Name, MapEntitiesByName ? EntityData.PrimaryNameField : null);
                    if (matchingSystemEntities.Count > 0) WriteVerbose($"Multiple entities for '{entityInfo.Name}' found ...");
                    if (matchingSystemEntities.Count == 0) 
                    { 
                        WriteWarning($"Entity '{entityInfo.Name}' not found!"); 
                        continue; 
                    }

                    int processedSharings = 0;
                    var entity = matchingSystemEntities.First();
                    foreach (var share in entityInfo.Sharings)
                    {
                        WriteProgress(new ProgressRecord(1, "Importing", $"Importing share '{share.Name}' ...") { PercentComplete = 100 * processedSharings / entityInfo.Sharings.Length });

                        var principal = GetPrincipal(share);
                        if (principal == null)
                        {
                            WriteWarning($"Principal '{share.Name}' not found!");
                            continue;
                        }

                        var accessRights = new PrincipalAccess { Principal = principal.ToEntityReference(), AccessMask = share.AccessMask };
                        var grantRequest = new GrantAccessRequest { Target = entity.ToEntityReference(), PrincipalAccess = accessRights };

                        Connection.Execute(grantRequest);
                    }
                }                
                processedEntities++;
            }
        }

        private Entity GetPrincipal(ShareInfo share)
        {
            var nameField = share.LogicalName == "systemuser" ? "fullname" : "name";
            var principal = MapPrincipalsByName
                ? _principals.SingleOrDefault(p => p[nameField].ToString() == share.Name)
                : _principals.SingleOrDefault(p => p.Id == share.Id);

            if(principal == null)
            {
                var principals = Connection.GetMatchingEntities(share.LogicalName, share.Id, share.Name, MapPrincipalsByName ? nameField : null);
                if (principals.Count == 0) WriteWarning($"Principal '{share.Name}' not found!");
                if (principals.Count > 0) WriteVerbose($"Multiple principals for '{share.Name}' found ...");

                principal = principals.FirstOrDefault();
                if(principal != null) _principals.Add(principal);
            }


            return principal;
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
        /// <para type="description">Set, if the principals should get mapped by name instead of the id.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter MapPrincipalsByName { get; set; }
    }
}
