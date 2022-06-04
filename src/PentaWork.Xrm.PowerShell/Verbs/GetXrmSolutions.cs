using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    #region DTOs
    /// <summary>
    /// <para type="synopsis">Object to hold solution information.</para>
    /// <para type="description">
    /// Holds the following properties: Name, UniqueName, Version, Publisher
    /// </para>
    /// </summary>
    public class SolutionInfo
    {
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public Version Version { get; set; }
        public string Publisher { get; set; }
    }
    #endregion

    /// <summary>
    /// <para type="synopsis">Gets a list of all installed and visible solutions in the target system.</para>
    /// <para type="description">
    /// The function returns a custom object with the following properties: Name, UniqueName, Version, Publisher
    /// </para>
    /// </summary>
    /// <example>
    /// <code>Get-CrmConnection -InteractiveMode | Get-XrmSolutions</code>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmSolutions")]
    [OutputType(typeof(SolutionInfo))]
    public class GetXrmSolutions : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var solutionQuery = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "publisherid", "installedon", "version", "versionnumber", "uniquename", "friendlyname", "isvisible" }),
                Criteria = new FilterExpression()
            };
            solutionQuery.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);

            Connection
                .RetrieveMultiple(solutionQuery)
                .Entities
                .Select(e => new SolutionInfo {
                    Name = e.Attributes["friendlyname"].ToString(),
                    UniqueName = e.Attributes["uniquename"].ToString(),
                    Version = new Version(e.Attributes["version"].ToString()),
                    Publisher = ((EntityReference)e.Attributes["publisherid"]).Name
                })
                .ToList()
                .ForEach(WriteObject);
        }

        /// <summary>
        /// <para type="description">The connection to the XRM Organization (Get-CrmConnection)</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public CrmServiceClient Connection { get; set; }
    }
}
