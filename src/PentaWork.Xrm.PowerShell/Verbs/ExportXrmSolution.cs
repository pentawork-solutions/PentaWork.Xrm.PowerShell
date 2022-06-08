using System;
using System.IO;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Management.Automation;
using PentaWork.Xrm.PowerShell.Common;
using System.Linq;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Export a solution from the target system.</para>
    /// <para type="description">
    /// This function is able to export a solution from a target system. The results of the <c>Get-XrmSolutions</c> function can be piped into this one.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -InteractiveMode</para>
    /// <para>Get-XrmSolutions -Connection $conn | Where-Object {$_.Name -like "TestSolution*"} | Export-XrmSolution -Connection $conn -Managed -ExportPath .\</para>
    /// </example>
    [OutputType(typeof(FileInfo))]
    [Cmdlet(VerbsData.Export, "XrmSolution")]
    public class ExportXrmSolution : PSCmdlet
    {
        private readonly ConsoleLogger _logger = new ConsoleLogger();

        protected override void ProcessRecord()
        {
            if (PublishAll) Connection.Execute(new PublishAllXmlRequest());

            var solutionVersion = RetrieveSolutionVersion();
            var solutionFilePath = Path.Combine(ExportPath, $"{UniqueName} - {solutionVersion} - {Connection.ConnectedOrgFriendlyName} - { (Managed ? "managed" : "unmanaged") }.zip");

            if (File.Exists(solutionFilePath) && !Force) _logger.Warn("Solution already existing in target path. Skipping ...");
            else
            {
                _logger.Info($"Exporting solution '{UniqueName}' ({solutionVersion}) from '{Connection.ConnectedOrgFriendlyName}'...");

                var exportSolutionRequest = new ExportSolutionRequest();
                exportSolutionRequest.Managed = Managed;
                exportSolutionRequest.SolutionName = UniqueName;

                var exportSolutionResponse = (ExportSolutionResponse)Connection.Execute(exportSolutionRequest);
                var exportBytes = exportSolutionResponse.ExportSolutionFile;

                File.WriteAllBytes(solutionFilePath, exportBytes);
            }

            WriteObject(new FileInfo(solutionFilePath));
        }

        private string RetrieveSolutionVersion()
        {
            var solutionQuery = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "publisherid", "installedon", "version", "versionnumber", "friendlyname" }),
                Criteria = new FilterExpression()
            };
            solutionQuery.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);

            var entities = Connection.Query(solutionQuery, true);
            return entities.SingleOrDefault()?.Attributes["version"].ToString();
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
        /// <para type="description">The unique name of the solution to export.).</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string UniqueName { get; set; }

        /// <summary>
        /// <para type="description">The path to export the solution to.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ExportPath { get; set; }

        /// <summary>
        /// <para type="description">Export as managed solution.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Managed { get; set; }

        /// <summary>
        /// <para type="description">Publish all before exporting.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter PublishAll { get; set; }

        /// <summary>
        /// <para type="description">Force export, if version is already existing in target path.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }
    }
}
