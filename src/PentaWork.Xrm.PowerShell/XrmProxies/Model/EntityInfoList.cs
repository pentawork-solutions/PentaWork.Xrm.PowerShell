using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class EntityInfoList : List<EntityInfo>
    {
        private readonly UniqueNameDictionary _entityNameDic = new UniqueNameDictionary();

        public EntityInfoList(List<EntityMetadata> entityMetadataList, List<Entity> systemForms, List<ActionInfo> actions)
        {
            // First parse all entity information
            ParseEntityMetadata(entityMetadataList, actions);
            // Then parse all relation information - The relation information
            // holds a reference to the related entity information. Thats why 
            // all entity information get parsed at a whole in the previous step.
            ParseEntityRelationAttributes(entityMetadataList);
            LoadSystemForms(systemForms);
        }

        private void ParseEntityMetadata(List<EntityMetadata> entityMetadataList, List<ActionInfo> actions)
        {
            AddRange(entityMetadataList.Select(e => new EntityInfo(e, actions.Where(a => a.PrimaryEntity == e.LogicalName).ToList(), _entityNameDic.GetUniqueName(e))));
        }

        private void ParseEntityRelationAttributes(List<EntityMetadata> entityMetadataList)
        {
            foreach (var entityMetadata in entityMetadataList)
            {
                var parsedEntityInfo = this.Single(e => e.LogicalName == entityMetadata.LogicalName);
                foreach (var manyToManyRelation in entityMetadata.ManyToManyRelationships.OrderBy(r => r.SchemaName))
                {
                    var relatedLogicalName = manyToManyRelation.Entity1LogicalName != parsedEntityInfo.LogicalName ? manyToManyRelation.Entity1LogicalName : manyToManyRelation.Entity2LogicalName;
                    var relatedEntityInfo = this.SingleOrDefault(e => e.LogicalName == relatedLogicalName);
                    if (relatedEntityInfo != null)
                        parsedEntityInfo.AddManyToManyRelationInfo(relatedEntityInfo, manyToManyRelation);
                }

                foreach(var oneToManyRelation in entityMetadata.OneToManyRelationships.OrderBy(r => r.SchemaName))
                {
                    var relatedLogicalName = oneToManyRelation.ReferencingEntity;
                    var relatedEntityInfo = this.SingleOrDefault(e => e.LogicalName == relatedLogicalName);
                    if(relatedEntityInfo != null)
                        parsedEntityInfo.AddOneToManyRelationInfo(relatedEntityInfo, oneToManyRelation);
                }
            }
        }

        private void LoadSystemForms(List<Entity> systemForms)
        {
            foreach (var systemForm in systemForms.OrderBy(e => e.LogicalName))
            {
                var parsedEntityInfo = this.SingleOrDefault(e => e.LogicalName == systemForm["objecttypecode"] as string);
                if(parsedEntityInfo != null) parsedEntityInfo.AddFormInfo(systemForm);
            }
        }
    }
}
