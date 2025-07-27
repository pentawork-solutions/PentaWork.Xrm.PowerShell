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
            var pluginGraphAnalyzer = new PluginGraphAnalyzer();
            pluginGraphAnalyzer.AnalyzeSystem(Connection, SolutionInfo?.Id);

            var l = "";
        }


        /*  private IDictionary<string, byte[]> DownloadPackages(IEnumerable<PluginStepInfo> pluginStepInfos)
          {
              var packageIds = pluginStepInfos
                  .Where(p => p.Plugin != null)
                  .Select(p => (p.Plugin!.PackageName, p.Plugin.PackageFileId))
                  .DistinctBy(p => p.PackageFileId);
              return packageIds
                  .Select(p => (p.PackageName!, DownloadFile(new EntityReference("pluginpackage", p.PackageFileId!.Value), "package")))
                  .ToDictionary(p => p.Item1, p => p.Item2);
          } */



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
        public SolutionInfo? SolutionInfo { get; set; }
    }
}
