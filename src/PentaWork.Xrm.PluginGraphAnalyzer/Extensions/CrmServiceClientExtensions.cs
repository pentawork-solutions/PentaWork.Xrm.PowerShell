using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Extensions
{
    internal static class CrmServiceClientExtensions
    {
        public static List<Entity> QueryEntity(this CrmServiceClient client, string logicalName, bool getAll, params ConditionExpression[] conditions)
        {
            var query = new QueryExpression(logicalName);
            query.ColumnSet = new ColumnSet(true);
            conditions?.ToList().ForEach(query.Criteria.AddCondition);

            return client.Query(query, getAll);
        }

        public static List<Entity> Query(this CrmServiceClient client, QueryExpression query, bool getAll = false)
        {
            var entities = new List<Entity>();

            if (query.PageInfo == null) query.PageInfo = new PagingInfo { Count = 1000, PageNumber = 1 };
            var response = client.RetrieveMultiple(query);

            entities.AddRange(response.Entities);
            if (getAll && response.MoreRecords)
            {
                while (response.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    response = client.RetrieveMultiple(query);
                    entities.AddRange(response.Entities);
                }
            }

            return entities;
        }

        public static IEnumerable<PluginStepInfo> GetPluginSteps(this CrmServiceClient connection, IEnumerable<ComponentInfo>? componentInfos)
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

            return connection
                .Query(query, true)
                .Select(e => new PluginStepInfo
                {
                    Id = e.Id,
                    Mode = ((OptionSetValue)e["mode"]).Value,
                    Stage = ((OptionSetValue)e["stage"]).Value,
                    Rank = (int)e["rank"],
                    StateCode = ((OptionSetValue)e["statecode"]).Value,
                    StatusCode = ((OptionSetValue)e["statuscode"]).Value,
                    AsyncAutoDelete = (bool)e["asyncautodelete"],
                    Name = (string)e["name"],
                    Category = e.Contains("category") ? (string)e["category"] : null,
                    SdkMessage = e.AV<string>("sdkmessage.name"),
                    FilteringAttributes = e.Contains("filteringattributes") ? (string)e["filteringattributes"] : null,
                    PrimaryEntityName = e.AV<string>("sdmessagefilter.primaryobjecttypecode"),
                    SecondaryEntityName = e.AV<string>("sdmessagefilter.secondaryobjecttypecode"),
                    Plugin = new PluginInfo
                    {
                        Id = ((EntityReference)e["eventhandler"]).Id,
                        PlugintypeExportKey = e.AV<string>("plugintype.plugintypeexportkey"),
                        TypeName = e.AV<string>("plugintype.typename"),
                        AssemblyInfo = new AssemblyInfo
                        {
                            Id = e.AV<EntityReference>("plugintype.pluginassemblyid")?.Id ?? Guid.Empty,
                            Name = e.AV<string>("pluginassembly.name"),
                        },
                        PackageInfo = e.AV<EntityReference>("pluginassembly.packageid") != null
                            ? new PackageInfo
                            {
                                Id = e.AV<EntityReference>("pluginassembly.packageid")!.Id,
                                Name = e.AV<string>("pluginpackage.name"),
                            }
                            : null
                    }
                }).ToList();
        }

        public static byte[] DownloadFile(this CrmServiceClient connection, EntityReference entityReference, string attributeName)
        {
            var initializeFileBlocksDownloadRequest = new InitializeFileBlocksDownloadRequest
            {
                Target = entityReference,
                FileAttributeName = attributeName
            };
            var initializeFileBlocksDownloadResponse = (InitializeFileBlocksDownloadResponse)connection.Execute(initializeFileBlocksDownloadRequest);

            var fileContinuationToken = initializeFileBlocksDownloadResponse.FileContinuationToken;
            var fileSizeInBytes = initializeFileBlocksDownloadResponse.FileSizeInBytes;
            var fileBytes = new List<byte>((int)fileSizeInBytes);

            var offset = 0L;
            var blockSizeDownload = !initializeFileBlocksDownloadResponse.IsChunkingSupported ? fileSizeInBytes : 4 * 1024 * 1024;
            if (fileSizeInBytes < blockSizeDownload) blockSizeDownload = fileSizeInBytes;

            while (fileSizeInBytes > 0)
            {
                var downLoadBlockRequest = new DownloadBlockRequest()
                {
                    BlockLength = blockSizeDownload,
                    FileContinuationToken = fileContinuationToken,
                    Offset = offset
                };
                var downloadBlockResponse = (DownloadBlockResponse)connection.Execute(downLoadBlockRequest);
                fileBytes.AddRange(downloadBlockResponse.Data);

                fileSizeInBytes -= (int)blockSizeDownload;
                offset += blockSizeDownload;
            }
            return fileBytes.ToArray();
        }
    }
}
