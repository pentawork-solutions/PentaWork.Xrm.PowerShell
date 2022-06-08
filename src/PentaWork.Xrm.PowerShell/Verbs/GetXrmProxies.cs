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
using PentaWork.Xrm.PowerShell.XrmProxies.Templates.Javascript;
using PentaWork.Xrm.PowerShell.XrmProxies.Templates.CSharp;
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
    /// <code>Get-CrmConnection -Interactive | Get-XrmProxies -FakeNamespace FakeNamespace -ProxyNamespace ProxyNamespace -OutputPath .\output -Clear</code>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmProxies")]
    public class GetXrmProxies : PSCmdlet
    {
        private readonly ConsoleLogger _logger = new ConsoleLogger();

        protected override void ProcessRecord()
        {
            if (Clear && OutputPath.Exists) Directory.Delete(OutputPath.FullName, true);

            var sdkMessages = GetAllSdkMessages();
            var actions = GetAllActions();
            var entityMetadata = GetAllEntityMetadata(sdkMessages);
            var systemForms = GetAllSystemForms();
            var entityInfoList = new EntityInfoList(entityMetadata, systemForms, actions);

            EnsureFolder(OutputPath.FullName);
            EnsureFolder(CSOutputPath);
            EnsureFolder(TSOutputPath);

            GenerateBaseClasses();
            GenerateAllCSharp(entityInfoList);
            GenerateAllJavascript(entityInfoList);
        }

        private void EnsureFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        }

        private List<Entity> GetAllSdkMessages()
        {
            _logger.Info("Getting SDK Messages ...");

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
            _logger.Info("Getting available Actions ...");

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
            _logger.Info("Getting metadata ...");
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
            _logger.Info("Getting system forms ...");

            var query = new QueryExpression("systemform");
            query.Criteria.AddCondition("objecttypecode", ConditionOperator.NotEqual, "none");
            query.ColumnSet = new ColumnSet(true);

            return Connection.Query(query, true);
        }

        private void GenerateBaseClasses()
        {
            if(UseBaseProxy)
            {
                WriteVerbose("Generating Base Proxy ...");
                var baseProxyTemplate = new BaseProxy { ProxyNamespace = ProxyNamespace };
                File.WriteAllText(Path.Combine(CSOutputPath, "BaseProxy.cs"), baseProxyTemplate.TransformText());
            }            

            WriteVerbose("Generating Attributes ...");
            var attributesTemplate = new Attributes { ProxyNamespace = ProxyNamespace };
            File.WriteAllText(Path.Combine(CSOutputPath, "Attributes.cs"), attributesTemplate.TransformText());

            var assemblyInfoAddition = new AssemblyInfoAddition();
            File.WriteAllText(Path.Combine(CSOutputPath, "AssemblyInfoAddition.cs"), assemblyInfoAddition.TransformText());

            WriteVerbose("Generating Extensions ...");
            EnsureFolder(Path.Combine(CSOutputPath, "Extensions"));

            var enumExtensionTemplate = new EnumExtensions { ProxyNamespace = ProxyNamespace };
            File.WriteAllText(Path.Combine(CSOutputPath, "Extensions", "EnumExtensions.cs"), enumExtensionTemplate.TransformText());
        }

        private void GenerateAllCSharp(EntityInfoList entityInfoList)
        {
            _logger.Info("[CS] Generating Proxy & Fake Classes ...");
            EnsureFolder(Path.Combine(CSOutputPath, "Entities"));
            EnsureFolder(Path.Combine(OutputPath.FullName, "Fake"));
            foreach (var entityInfo in entityInfoList)
            {
                var proxyTemplate = new ProxyClass { EntityInfo = entityInfo, ProxyNamespace = ProxyNamespace, UseBaseProxy = UseBaseProxy };
                File.WriteAllText(Path.Combine(CSOutputPath, "Entities", $"{entityInfo.UniqueDisplayName}.cs"), proxyTemplate.TransformText());

                var fakeTemplate = new Fake { EntityInfo = entityInfo, ProxyNamespace = ProxyNamespace, FakeNamespace = FakeNamespace };
                File.WriteAllText(Path.Combine(OutputPath.FullName, "Fake", $"{entityInfo.UniqueDisplayName}.cs"), fakeTemplate.TransformText());
            }

            _logger.Info("[CS] Generating Relation Classes ...");
            EnsureFolder(Path.Combine(CSOutputPath, "Relations"));
            foreach (var relationClassInfo in entityInfoList.SelectMany(e => e.ManyToManyRelationList))
            {
                var proxyTemplate = new RelationProxyClass { RelationClassInfo = relationClassInfo, ProxyNamespace = ProxyNamespace, UseBaseProxy = UseBaseProxy };
                File.WriteAllText(Path.Combine(CSOutputPath, "Relations", $"{relationClassInfo.UniqueDisplayName}.cs"), proxyTemplate.TransformText());
            }
        }

        private void GenerateAllJavascript(EntityInfoList entityInfoList)
        {
            _logger.Info("[TS] Generating Proxy Base Types ...");
            var proxyTypesTemplate = new ProxyTypes();
            File.WriteAllText(Path.Combine(TSOutputPath, "ProxyTypes.ts"), proxyTypesTemplate.TransformText());

            _logger.Info($"[TS] Generating Entity Proxies...");
            EnsureFolder(Path.Combine(TSOutputPath, "Entities"));
            foreach (var entityInfo in entityInfoList)
            {
                var proxyTemplate = new ProxyClassJS { EntityInfo = entityInfo };
                File.WriteAllText(Path.Combine(TSOutputPath, "Entities", $"{entityInfo.UniqueDisplayName}.ts"), proxyTemplate.TransformText());
            }

            _logger.Info($"[TS] Generating Attribute Name Modules...");
            EnsureFolder(Path.Combine(TSOutputPath, "Attributes"));
            foreach (var entityInfo in entityInfoList)
            {
                var attributeModule = new AttributeJS { EntityInfo = entityInfo };
                File.WriteAllText(Path.Combine(TSOutputPath, "Attributes", $"{entityInfo.UniqueDisplayName}.ts"), attributeModule.TransformText());
            }

            _logger.Info($"[TS] Generating Form Info Modules...");
            EnsureFolder(Path.Combine(TSOutputPath, "FormInfos"));
            foreach (var entityInfo in entityInfoList)
            {
                var formInfoModule = new FormInfosJS { EntityInfo = entityInfo };
                File.WriteAllText(Path.Combine(TSOutputPath, "FormInfos", $"{entityInfo.UniqueDisplayName}.ts"), formInfoModule.TransformText());
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
