using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Imports a solution into the target system.</para>
    /// <para type="description">
    /// This function is able to import a solution into a target system. It is possible to pipe the result of <c>Export-XrmSolution</c> into this function.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -Interactive</para>
    /// <para>$conn2 = Get-CrmConnection -Interactive</para>
    /// <para>Export-XrmSolution -Connection $conn -UniqueName TestSolution -Managed -ExportPath | Import-XrmSolution -Connection $conn2 -Overwrite</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "XrmSolution")]
    public class ImportXrmSolution : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var (solutionName, solutionVersion) = GetSolutionInfo();
            var targetVersion = RetrieveSolutionVersion(solutionName);

            if (targetVersion != null) Console.WriteLine($"Found version {targetVersion} in target system ...", new[] { "" });
            else Console.WriteLine("No version found in target system ...");

            if(targetVersion != solutionVersion || Force)
            {
                if (Delete) RemoveSolution(solutionName);

                byte[] solution = File.ReadAllBytes(SolutionFile.FullName);
                Console.WriteLine($"Importing solution '{solutionName}' to '{Connection.ConnectedOrgFriendlyName}'...");

                var importSolutionRequest = new ImportSolutionRequest();
                importSolutionRequest.CustomizationFile = solution;
                importSolutionRequest.OverwriteUnmanagedCustomizations = Overwrite;
                importSolutionRequest.PublishWorkflows = PublishWorkflows;

                Connection.Execute(importSolutionRequest);
                Console.WriteLine($"Importing done!");
            }
            else
            {
                Console.WriteLine($"Solution '{solutionName}' ({solutionVersion}) already deployed to target system!");
            }
        }

        private (string, Version) GetSolutionInfo()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            ZipFile.ExtractToDirectory(SolutionFile.FullName, tempPath);

            var solutionXml = XDocument.Load(Path.Combine(tempPath, "solution.xml"));
            var uniqueName = solutionXml.XPathSelectElements("ImportExportXml/SolutionManifest/UniqueName").Single().Value;
            var version = new Version(solutionXml.XPathSelectElements("ImportExportXml/SolutionManifest/Version").Single().Value);

            Directory.Delete(tempPath, true);
            return (uniqueName, version);
        }

        private Version RetrieveSolutionVersion(string solutionName)
        {
            var solutionQuery = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "version", "versionnumber" }),
                Criteria = new FilterExpression()
            };
            solutionQuery.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionName);

            var response = Connection.RetrieveMultiple(solutionQuery).Entities;
            if (response != null && response.Count == 1)
            {
                return new Version(response[0].Attributes["version"].ToString());
            }
            return null;
        }

        private void RemoveSolution(string solutionName)
        {
            Console.WriteLine($"Deleting solution ...");
            var query = new QueryExpression("solution");
            query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionName);
            query.ColumnSet = new ColumnSet(new[] { "solutionid", "friendlyname" });

            var solution = Connection.RetrieveMultiple(query).Entities.SingleOrDefault();
            if (solution != null)
            {
                Connection.Delete("solution", solution.Id);
            }
        }

        /// <summary>
        /// <para type="description">The connection to the XRM Organization (Get-CrmConnection)</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public CrmServiceClient Connection { get; set; }

        /// <summary>
        /// <para type="description">The path to the solution zip file.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public FileInfo SolutionFile { get; set; }

        /// <summary>
        /// <para type="description">Delete before installing.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Delete { get; set; }

        /// <summary>
        /// <para type="description">Overwrite unmanaged customizations.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Overwrite { get; set; }

        /// <summary>
        /// <para type="description">Publish all workflows.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter PublishWorkflows { get; set; }

        /// <summary>
        /// <para type="description">Force install even, if version is already deployed.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }
    }
}
