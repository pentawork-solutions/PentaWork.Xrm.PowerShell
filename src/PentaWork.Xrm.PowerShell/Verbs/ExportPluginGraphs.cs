using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
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
            IEnumerable<ComponentInfo> solutionComponents = null;
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

            var l = "";
        }

        private IEnumerable<PluginStepInfo> GetPluginSteps(IEnumerable<ComponentInfo> componentInfos)
        {
            var query = new QueryExpression("sdkmessageprocessingstep");
            query.ColumnSet = new ColumnSet(true);

            // SDK Message Filters
            query.LinkEntities.Add(new LinkEntity("sdkmessageprocessingstep", "sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("primaryobjecttypecode", "secondaryobjecttypecode");
            query.LinkEntities[0].EntityAlias = "sdmessagefilter";

            // PluginTypes
            query.LinkEntities.Add(new LinkEntity("sdkmessageprocessingstep", "plugintype", "eventhandler", "plugintypeid", JoinOperator.Inner));
            query.LinkEntities[1].Columns.AddColumns("plugintypeexportkey", "typename", "assemblyname", "pluginassemblyid");
            query.LinkEntities[1].EntityAlias = "plugintype";

            // SDK Message
            query.LinkEntities.Add(new LinkEntity("sdkmessageprocessingstep", "sdkmessage", "sdkmessageid", "sdkmessageid", JoinOperator.Inner));
            query.LinkEntities[2].Columns.AddColumns("name");
            query.LinkEntities[2].EntityAlias = "sdkmessage";

            // Plugin Assembly
            query.LinkEntities.Add(new LinkEntity("plugintype", "pluginassembly", "plugintype.pluginassemblyid", "pluginassemblyid", JoinOperator.Inner));
            query.LinkEntities[3].Columns.AddColumns("name", "packageid");
            query.LinkEntities[3].EntityAlias = "pluginassembly";

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
                        AssemblyName = (string)((AliasedValue)e["plugintype.assemblyname"]).Value,
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
