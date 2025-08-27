using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PluginGraph;
using System.Management.Automation;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    [OutputType(typeof(EntityData))]
    [Cmdlet(VerbsData.Export, "PluginGraphs")]
    public class ExportPluginGraphs : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            if (Clear && OutputPath.Exists) Directory.Delete(OutputPath.FullName, true);
            if (!Directory.Exists(OutputPath.FullName)) Directory.CreateDirectory(OutputPath.FullName);

            var pluginGraphAnalyzer = new PluginGraphAnalyzer();
            pluginGraphAnalyzer.Progress += (s, t, n) =>
            {
                if (s == t) WriteProgress(new ProgressRecord(0, "Analyzing", "Done!") { RecordType = ProgressRecordType.Completed });
                else WriteProgress(new ProgressRecord(0, "Analyzing", $"Analyzing plugin step '{n}' ...") { PercentComplete = s * 100 / t });
            };

            WriteVerbose("Loading plugin steps and analyzing...");
            var entityGraphList = pluginGraphAnalyzer.AnalyzeSystem(Connection, SolutionInfo.Id, Namespaces.ToString(), Log);

            WriteVerbose("Writing markdown files....");
            entityGraphList.ForEach(e => File.WriteAllText(Path.Combine(OutputPath.FullName, $"{e.EntityName}.md"), e.ToMarkdown()));
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
        /// <para type="description">If a solution info is given (use Get-XrmSolutions), only plugin steps and plugin packages within the solution get analyzed.
        /// Otherwise all available in the connected system.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public SolutionInfo SolutionInfo { get; set; }

        /// <summary>
        /// <para type="description">The namespaces which should get analyzed.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Namespaces { get; set; }

        /// <summary>
        /// <para type="description">The output path for the markdown files.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public DirectoryInfo OutputPath { get; set; }

        /// <summary>
        /// <para type="description">Clear the output folder before generating markdowns.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Clear { get; set; }

        /// <summary>
        /// <para type="description">Log the VM execution for each Plugin.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Log { get; set; }
    }
}
