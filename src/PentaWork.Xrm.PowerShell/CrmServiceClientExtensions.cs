using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Collections.Generic;

namespace PentaWork.Xrm.PowerShell.Common
{
    internal static class CrmServiceClientExtensions
    {
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

        public static EntityMetadata GetMetadata(this CrmServiceClient client, string logicalName)
        {
            var request = new RetrieveEntityRequest
            {
                LogicalName = logicalName,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                RetrieveAsIfPublished = true
            };
            return ((RetrieveEntityResponse)client.Execute(request)).EntityMetadata;
        }
    }
}
