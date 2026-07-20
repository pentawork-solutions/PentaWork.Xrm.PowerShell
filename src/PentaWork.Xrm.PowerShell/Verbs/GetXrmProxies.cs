using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PowerShell.XrmProxies.Model;
using PentaWork.Xrm.PowerShell.XrmProxies.Templates;
using PentaWork.Xrm.PowerShell.XrmProxies;
using PentaWork.Xrm.PowerShell.Common;

namespace PentaWork.Xrm.PowerShell
{
    /// <summary>
    /// <para type="synopsis">Creates C# and TypeScript proxy classes and fake classes for C# unit testing.</para>
    /// <para type="description">
    /// The C# proxy classes contain all entity attributes, optionsets, relation properties and attribute/schema names.
    /// The relations are loadable with the "LoadProperty" method of the <c>OrganizationServiceContext</c>.
    /// The fake classes are created to be used with <c>FakeXrmEasy</c>. 
    /// They contain pre defined fake relationships and a static create method to create an instance of an entity with some random data (generated with <c>Bogus</c>)
    /// </para>
    /// </summary>
    /// <example>
    /// <code>Get-CrmConnection -InteractiveMode | Get-XrmProxies -FakeNamespace FakeNamespace -ProxyNamespace ProxyNamespace -OutputPath .\output -Clear</code>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmProxies")]
    public class GetXrmProxies : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            if (Clear && OutputPath.Exists) Directory.Delete(OutputPath.FullName, true);

            var sdkMessages = GetAllSdkMessages();
            var actions = GetAllActions();
            var entityMetadata = GetAllEntityMetadata(sdkMessages);
            var systemForms = GetAllSystemForms();

            if(!string.IsNullOrEmpty(Solution))
            {
                var solutionEntities = GetSolutionEntityNames(Solution);
                IncludeEntities.AddRange(entityMetadata.Where(e => solutionEntities.Any(s => s == e.MetadataId)).Select(e => e.LogicalName));
            }
            var filteredEntityMetadata = GetFilteredEntityMetaData(entityMetadata);
            var entityInfoList = new EntityInfoList(filteredEntityMetadata, systemForms, actions);

            EnsureFolder(OutputPath.FullName);
            EnsureFolder(CSOutputPath);
            EnsureFolder(TSOutputPath);

            GenerateBaseClasses();
            GenerateAllCSharp(entityInfoList);
            GenerateAllJavascript(entityInfoList);

            WriteProgress(new ProgressRecord(0, "Generating", "Done!") { RecordType = ProgressRecordType.Completed });
        }

        private void EnsureFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        }

        private List<Entity> GetAllSdkMessages()
        {
            WriteProgress(new ProgressRecord(0, "Generating", $"Getting SDK Messages ...") { PercentComplete = 0 });

            var query = new QueryExpression("sdkmessage");
            query.LinkEntities.Add(new LinkEntity("sdkmessage", "sdkmessagefilter", "sdkmessageid", "sdkmessageid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("primaryobjecttypecode", "secondaryobjecttypecode");
            query.LinkEntities[0].EntityAlias = "sdmessagefilter";

            query.ColumnSet = new ColumnSet("name", "isprivate");
            query.Criteria.AddCondition("isprivate", ConditionOperator.Equal, false);

            return Connection.Query(query, true);
        }

        private List<ActionInfo> GetAllActions()
        {
            WriteProgress(new ProgressRecord(0, "Generating", $"Getting available Actions ...") { PercentComplete = 10 });

            var query = new QueryExpression("workflow");
            query.LinkEntities.Add(new LinkEntity("workflow", "sdkmessage", "sdkmessageid", "sdkmessageid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("name");
            query.LinkEntities[0].EntityAlias = "sdkmessage";

            query.ColumnSet = new ColumnSet("name", "sdkmessageid", "primaryentity");
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 1);
            query.Criteria.AddCondition("type", ConditionOperator.Equal, 1);
            query.Criteria.AddCondition("category", ConditionOperator.Equal, 3);

            var actionNameDic = new UniqueNameDictionary();
            return Connection.Query(query, true)
                .Select(e => new ActionInfo(actionNameDic.GetUniqueName((string)e["name"], ((EntityReference)e["sdkmessageid"]).Id.ToString()), (string)((AliasedValue)e["sdkmessage.name"]).Value, (string)e["primaryentity"]))
                .ToList();
        }

        private List<EntityMetadata> GetAllEntityMetadata(List<Entity> sdkMessages)
        {
            WriteProgress(new ProgressRecord(0, "Generating", $"Getting metadata ...") { PercentComplete = 20 });

            var request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                RetrieveAsIfPublished = false
            };

            return ((RetrieveAllEntitiesResponse)Connection.Execute(request))
                .EntityMetadata
                .Where(e => 
                       !e.IsIntersect.GetValueOrDefault()
                    && !e.IsPrivate.GetValueOrDefault()
                    // Remove all Entities which are not useable with any non private SDK message
                    && sdkMessages.Any(s => ((AliasedValue)s["sdmessagefilter.primaryobjecttypecode"]).Value as string == e.LogicalName 
                                         || ((AliasedValue)s["sdmessagefilter.secondaryobjecttypecode"]).Value as string == e.LogicalName))
                .ToList();
        }

        private List<Entity> GetAllSystemForms()
        {
            WriteProgress(new ProgressRecord(0, "Generating", $"Getting system forms ...") { PercentComplete = 30 });

            var query = new QueryExpression("systemform");
            query.Criteria.AddCondition("objecttypecode", ConditionOperator.NotEqual, "none");
            query.ColumnSet = new ColumnSet(true);

            return Connection.Query(query, true);
        }

        private List<Guid> GetSolutionEntityNames(string solution)
        {
            WriteProgress(new ProgressRecord(0, "Generating", $"Getting Solution Entities ...") { PercentComplete = 35 });

            var solutionQuery = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "uniquename", "friendlyname", "isvisible" })
            };
            solutionQuery.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
            solutionQuery.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solution);

            var solutionId = (Connection
                .Query(solutionQuery, true)
                .SingleOrDefault()?.Id) ?? throw new Exception($"Solution with unique name '{solution}' not found!");

            var componentQuery = new QueryExpression
            {
                EntityName = "solutioncomponent",
                ColumnSet = new ColumnSet(new string[] { "objectid", "solutioncomponentid", "componenttype" })
            };
            componentQuery.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);
            componentQuery.Criteria.AddCondition("componenttype", ConditionOperator.Equal, 1); // Entity

            return Connection.Query(componentQuery, true).Select(c => new Guid(c.Attributes["objectid"].ToString())).ToList();
        }

        private List<EntityMetadata> GetFilteredEntityMetaData(List<EntityMetadata> entityMetadataList)
        {
            var filteredEntityMetadataList = new List<EntityMetadata>();
            if (IncludeEntities.Any())
                filteredEntityMetadataList = entityMetadataList.Where(e => IncludeEntities.Any(i => i == e.LogicalName)).ToList();
            else
                filteredEntityMetadataList = entityMetadataList;
            return filteredEntityMetadataList.Where(f => ExcludeEntities.All(e => e != f.LogicalName)).ToList();
        }

        private void GenerateBaseClasses()
        {
            if(UseBaseProxy)
            {
                WriteProgress(new ProgressRecord(0, "Generating", $"Generating Base Proxy ...") { PercentComplete = 40 });
                File.WriteAllText(Path.Combine(CSOutputPath, "BaseProxy.cs"), ScribanTemplateRenderer.Render("CSharp.BaseProxy", new { ProxyNamespace }));
            }

            WriteProgress(new ProgressRecord(0, "Generating", $"Generating Attributes ...") { PercentComplete = 40 });
            File.WriteAllText(Path.Combine(CSOutputPath, "Attributes.cs"), ScribanTemplateRenderer.Render("CSharp.Attributes", new { ProxyNamespace }));

            File.WriteAllText(Path.Combine(CSOutputPath, "AssemblyInfoAddition.cs"), ScribanTemplateRenderer.Render("CSharp.AssemblyInfoAddition", new { }));

            WriteProgress(new ProgressRecord(0, "Generating", $"Generating Extensions ...") { PercentComplete = 45 });
            EnsureFolder(Path.Combine(CSOutputPath, "Extensions"));

            File.WriteAllText(Path.Combine(CSOutputPath, "Extensions", "EnumExtensions.cs"), ScribanTemplateRenderer.Render("CSharp.EnumExtensions", new { ProxyNamespace }));
        }

        private void GenerateAllCSharp(EntityInfoList entityInfoList)
        {
            WriteProgress(new ProgressRecord(0, "Generating", $"[CS] Generating Proxy & Fake Classes ...") { PercentComplete = 50 });
            EnsureFolder(Path.Combine(CSOutputPath, "Entities"));
            EnsureFolder(Path.Combine(OutputPath.FullName, "Fake"));
            foreach (var entityInfo in entityInfoList)
            {
                var optionSetEnumsCs = string.Join(Environment.NewLine,
                    entityInfo.OptionSetList.Select(os => ScribanTemplateRenderer.Render("CSharp.OptionSet", new { OptionSetInfo = os })));
                File.WriteAllText(Path.Combine(CSOutputPath, "Entities", $"{entityInfo.UniqueDisplayName}.cs"),
                    // SwitchParameter has an implicit bool conversion that only the C# compiler applies -
                    // Scriban sees any non-null object as truthy, so the switch must be unwrapped to a
                    // real bool before it reaches the template, or {{ if UseBaseProxy }} is always true.
                    ScribanTemplateRenderer.Render("CSharp.ProxyClass", new { EntityInfo = entityInfo, ProxyNamespace, UseBaseProxy = (bool)UseBaseProxy, OptionSetEnumsCs = optionSetEnumsCs }));

                File.WriteAllText(Path.Combine(OutputPath.FullName, "Fake", $"{entityInfo.UniqueDisplayName}.cs"),
                    ScribanTemplateRenderer.Render("CSharp.Fake", new { EntityInfo = entityInfo, ProxyNamespace, FakeNamespace }));
            }

            WriteProgress(new ProgressRecord(0, "Generating", $"[CS] Generating Relation Classes ...") { PercentComplete = 65 });
            EnsureFolder(Path.Combine(CSOutputPath, "Relations"));
            // Group by intersect entity. It is possible, that multiple schemas are using the same intersect entity!
            foreach (var relationInfoGroup in entityInfoList.SelectMany(e => e.ManyToManyRelationList).GroupBy(e => e.IntersectEntityName))
            {
                var relationInfo = relationInfoGroup.First();
                File.WriteAllText(Path.Combine(CSOutputPath, "Relations", $"{relationInfo.UniqueIntersectDisplayName}.cs"),
                    ScribanTemplateRenderer.Render("CSharp.RelationProxyClass", new { RelationClassInfo = relationInfo, ProxyNamespace, UseBaseProxy = (bool)UseBaseProxy }));
            }
        }

        private void GenerateAllJavascript(EntityInfoList entityInfoList)
        {

            WriteProgress(new ProgressRecord(0, "Generating", $"[TS] Generating Proxy Base Types ...") { PercentComplete = 75 });
            File.WriteAllText(Path.Combine(TSOutputPath, "ProxyTypes.ts"), ScribanTemplateRenderer.Render("Javascript.ProxyTypes", new { }));

            WriteProgress(new ProgressRecord(0, "Generating", $"[TS] Generating Entity Proxies...") { PercentComplete = 85 });
            EnsureFolder(Path.Combine(TSOutputPath, "Entities"));
            foreach (var entityInfo in entityInfoList)
            {
                var optionSetEnumsJs = string.Join(Environment.NewLine,
                    entityInfo.OptionSetList.Select(os => ScribanTemplateRenderer.Render("Javascript.OptionSetJS", new { OptionSetInfo = os })));
                File.WriteAllText(Path.Combine(TSOutputPath, "Entities", $"{entityInfo.UniqueDisplayName}.ts"),
                    ScribanTemplateRenderer.Render("Javascript.ProxyClassJS", new { EntityInfo = entityInfo, OptionSetEnumsJs = optionSetEnumsJs }));
            }

            WriteProgress(new ProgressRecord(0, "Generating", $"[TS] Generating Attribute Name Modules...") { PercentComplete = 90 });
            EnsureFolder(Path.Combine(TSOutputPath, "Attributes"));
            foreach (var entityInfo in entityInfoList)
            {
                File.WriteAllText(Path.Combine(TSOutputPath, "Attributes", $"{entityInfo.UniqueDisplayName}.ts"),
                    ScribanTemplateRenderer.Render("Javascript.AttributeJS", new { EntityInfo = entityInfo }));
            }

            WriteProgress(new ProgressRecord(0, "Generating", $"[TS] Generating Form Info Modules...") { PercentComplete = 95 });
            EnsureFolder(Path.Combine(TSOutputPath, "FormInfos"));
            foreach (var entityInfo in entityInfoList)
            {
                File.WriteAllText(Path.Combine(TSOutputPath, "FormInfos", $"{entityInfo.UniqueDisplayName}.ts"),
                    ScribanTemplateRenderer.Render("Javascript.FormInfosJS", new { EntityInfo = entityInfo }));
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
        /// <para type="description">The namespace for all proxy classes.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ProxyNamespace { get; set; }

        /// <summary>
        /// <para type="description">The namespace for all fake classes.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string FakeNamespace { get; set; }

        /// <summary>
        /// <para type="description">Only create proxies for entities which are part of the given solution (uniquename).</para>
        /// </summary>
        [Parameter]
        public string Solution { get; set; }

        /// <summary>
        /// <para type="description">If used with the solution parameter, this list will add
        /// entities which are not part of the given solution.</para>
        /// </summary>
        [Parameter]
        public List<string> IncludeEntities { get; set; } = new List<string>();

        /// <summary>
        /// <para type="description">All entities in this list will be excluded in the proxy generation.</para>
        /// </summary>
        [Parameter]
        public List<string> ExcludeEntities { get; set; } = new List<string>();

        /// <summary>
        /// <para type="description">The output path for the proxy classes.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public DirectoryInfo OutputPath { get; set; }

        /// <summary>
        /// <para type="description">Clear the output folder before generating proxies.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Clear { get; set; }

        /// <summary>
        /// <para type="description">Use a custom base proxy class for the generated proxies.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter UseBaseProxy { get; set; }

        public string CSOutputPath => Path.Combine(OutputPath.FullName, "CSharp");
        public string TSOutputPath => Path.Combine(OutputPath.FullName, "TS");
    }
}
