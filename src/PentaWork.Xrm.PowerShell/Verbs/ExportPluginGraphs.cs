using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PluginGraph;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;
using PentaWork.Xrm.PowerShell.Common;
using System.Management.Automation;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    #region DTOs
    public class ComponentInfo
    {
        public ComponentInfo(Entity entity)
        {
            Id = entity.Id;
            ObjectId = (Guid)entity["objectid"];
            ComponentType = ((OptionSetValue)entity["componenttype"]).Value;
        }

        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public int ComponentType { get; set; }
    }
    #endregion
    [OutputType(typeof(EntityData))]
    [Cmdlet(VerbsData.Export, "PluginGraphs")]
    public class ExportPluginGraphs : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            IEnumerable<ComponentInfo>? solutionComponents = null;
            if (SolutionInfo != null)
            {
                // We are fetching the solution components first, instead of filtering the plugin types based on the solution id.
                // This way this module also works for unmanaged solutions -> Unmanaged Plugin Types are part of the default solution.
                WriteProgress(new ProgressRecord(0, "Generating", $"Retrieving plugin types for '{SolutionInfo.Name}' ...") { PercentComplete = 0 });
                solutionComponents = Connection
                    .QueryEntity("solutioncomponent", true, new ConditionExpression("solutionid", ConditionOperator.Equal, SolutionInfo.Id))
                    .Select(e => new ComponentInfo(e));
            }
            var pluginInfos = GetPluginSteps(solutionComponents);
            var pluginGraphAnalyzer = new PluginGraphAnalyzer(pluginInfos, null);
            pluginGraphAnalyzer.Analyze();

            var l = "";
        }

        private IEnumerable<PluginStepInfo> GetPluginSteps(IEnumerable<ComponentInfo>? componentInfos)
        {
            var query = new QueryExpression("sdkmessageprocessingstep");
            query.ColumnSet = new ColumnSet(true);

            // SDK Message Filters
            var linkedFilters = new LinkEntity("sdkmessageprocessingstep", "sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid", JoinOperator.Inner);
            query.LinkEntities.Add(linkedFilters);
            linkedFilters.Columns.AddColumns("primaryobjecttypecode", "secondaryobjecttypecode");
            linkedFilters.EntityAlias = "sdmessagefilter";

            // PluginTypes
            var linkedPluginTypes = new LinkEntity("sdkmessageprocessingstep", "plugintype", "eventhandler", "plugintypeid", JoinOperator.Inner);
            query.LinkEntities.Add(linkedPluginTypes);
            linkedPluginTypes.Columns.AddColumns("plugintypeexportkey", "typename", "assemblyname", "pluginassemblyid");
            linkedPluginTypes.EntityAlias = "plugintype";

            // SDK Message
            var linkedMessages = new LinkEntity("sdkmessageprocessingstep", "sdkmessage", "sdkmessageid", "sdkmessageid", JoinOperator.Inner);
            query.LinkEntities.Add(linkedMessages);
            linkedMessages.Columns.AddColumns("name");
            linkedMessages.EntityAlias = "sdkmessage";

            // Plugin Assembly
            var linkedAssemblies = new LinkEntity("plugintype", "pluginassembly", "pluginassemblyid", "pluginassemblyid", JoinOperator.Inner);
            linkedPluginTypes.LinkEntities.Add(linkedAssemblies);
            linkedAssemblies.Columns.AddColumns("name", "packageid");
            linkedAssemblies.EntityAlias = "pluginassembly";

            // Packages
            var linkedPackages = new LinkEntity("pluginassembly", "pluginpackage", "packageid", "pluginpackageid", JoinOperator.Inner);
            linkedAssemblies.LinkEntities.Add(linkedPackages);
            linkedPackages.Columns.AddColumns("name", "package");
            linkedPackages.EntityAlias = "pluginpackage";

            if (componentInfos?.Any() == true)
                query.Criteria.AddCondition("sdkmessageprocessingstepid", ConditionOperator.In,
                    componentInfos.Where(c => c.ComponentType == 92).Select(a => a.ObjectId.ToString()).ToArray());

            return Connection
                .Query(query, true)
                .Select(e => new PluginStepInfo
                {
                    Id = e.Id,
                    Mode = ((OptionSetValue)e["mode"]).Value,
                    Stage = ((OptionSetValue)e["stage"]).Value,
                    Rank = (int)e["rank"],
                    StateCode = ((OptionSetValue)e["statecode"]).Value,
                    Name = (string)e["name"],
                    Category = e.Contains("category")
                        ? (string)e["category"]
                        : null,
                    FilteringAttributes = e.Contains("filteringattributes")
                        ? (string)e["filteringattributes"]
                        : null,
                    Plugin = new PluginInfo
                    {
                        Id = ((EntityReference)e["eventhandler"]).Id,
                        PlugintypeExportKey = (string)((AliasedValue)e["plugintype.plugintypeexportkey"]).Value,
                        TypeName = (string)((AliasedValue)e["plugintype.typename"]).Value,
                        AssemblyName = (string)((AliasedValue)e["pluginassembly.name"]).Value,
                        PackageName = e.Contains("pluginpackage.name")
                            ? (string)((AliasedValue)e["pluginpackage.name"]).Value
                            : null,
                        PackageFileId = e.Contains("pluginpackage.package")
                            ? (Guid)((AliasedValue)e["pluginpackage.package"]).Value
                            : null,
                    },
                    SdkMessage = new SdkMessageInfo
                    {
                        Id = ((EntityReference)e["sdkmessageid"]).Id,
                        Name = (string)((AliasedValue)e["sdkmessage.name"]).Value
                    },
                    SdkFilter = e.Contains("sdkmessagefilterid")
                        ? new SdkFilterInfo
                        {
                            Id = ((EntityReference)e["sdkmessagefilterid"]).Id,
                            PrimaryObjectTypecode = (string)((AliasedValue)e["sdmessagefilter.primaryobjecttypecode"]).Value,
                            SecondaryObjectTypecode = e.Contains("sdmessagefilter.secondaryobjecttypecode")
                                ? (string)((AliasedValue)e["sdmessagefilter.secondaryobjecttypecode"]).Value
                                : null
                        }
                        : null
                }).ToList();
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
        public SolutionInfo? SolutionInfo { get; set; }
    }
}
