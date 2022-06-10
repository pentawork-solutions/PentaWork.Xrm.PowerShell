using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.ServiceModel;

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

        public static Entity TryRetrieve(this CrmServiceClient client, string logicalName, Guid id, ColumnSet columns)
        {
            Entity entity = null;
            try
            {
                entity = client.Retrieve(logicalName, id, columns);
            }
            catch (FaultException) { /*No entity found*/ }
            return entity;
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

        public static List<Entity> GetAll(this CrmServiceClient client, string logicalName, params string[] columns)
        {
            var query = new QueryExpression
            {
                EntityName = logicalName,
                ColumnSet = new ColumnSet(columns)
            };
            return client.Query(query, true);
        }

        public static List<Entity> GetEntitiesByName(this CrmServiceClient client, string logicalName, string nameField, string entityName)
        {
            var query = new QueryExpression
            {
                EntityName = logicalName,
                ColumnSet = new ColumnSet(true)
            };
            query.Criteria.AddCondition(nameField, ConditionOperator.Equal, entityName);
            return client.Query(query);
        }

        public static List<Entity> GetMatchingEntities(this CrmServiceClient client, string logicalName, Guid id, string name, string primaryNameField = null)
        {
            List<Entity> entities;
            if (!string.IsNullOrEmpty(primaryNameField))
            {
                entities = client.GetEntitiesByName(logicalName, primaryNameField, name);
            }
            else
            {
                var entity = client.TryRetrieve(logicalName, id, new ColumnSet());
                entities = entity != null
                    ? new List<Entity> { entity }
                    : new List<Entity>();
            }
            return entities;
        }
    }
}
