using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.PowerShell.Common;
using Microsoft.Xrm.Sdk.Messages;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Removes ALL entities matching the given parameters.</para>
    /// <para type="description">
    /// This function remoces all entities matching the given entity name and conditions.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -InteractiveMode</para>
    /// <para>$cond = [Microsoft.Xrm.Sdk.Query.ConditionExpression]::new("accountname", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, "test")</para>
    /// <para>Remove-XrmEntities -EntityName account -Conditions @($cond)</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "XrmEntities")]
    public class RemoveXrmEntities : PSCmdlet
    {
        private readonly ConsoleLogger _logger = new ConsoleLogger();

        protected override void ProcessRecord()
        {
            _logger.Info("Getting entities ...");
            var query = new QueryExpression
            {
                EntityName = EntityName,
                ColumnSet = new ColumnSet()
            };
            FilterConditions.ForEach(c => query.Criteria.AddCondition(c));
            var entities = Connection.Query(query, true);

            _logger.Info($"Deleting {entities.Count} entities ...");
            
            var deleteRequests = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                }
            };
            deleteRequests.Requests = new OrganizationRequestCollection();

            entities.ForEach(e => deleteRequests.Requests.Add(new DeleteRequest { Target = e.ToEntityReference() }));
            var response = (ExecuteMultipleResponse)Connection.Execute(deleteRequests);
            
            var hasErrors = response.Responses.Any(r => r.Fault != null);
            if (hasErrors)
            {
                foreach (var responseItem in response.Responses)
                {
                    if (responseItem.Fault != null)
                    {
                        var request = (DeleteRequest)deleteRequests.Requests[responseItem.RequestIndex];
                        _logger.Error($"{request.Target.Name} ({request.Target.Id}): {responseItem.Fault.Message}");
                    }
                }
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
        /// <para type="description">The entity name of the enitities to delete.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string EntityName { get; set; }

        /// <summary>
        /// <para type="description">Filter conditions to filter the entities which should get deleted.</para>
        /// </summary>
        [Parameter]
        public List<ConditionExpression> FilterConditions { get; set; } = new List<ConditionExpression>();
    }
}
