using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class EntityInfo
    {
        private readonly UniqueNameDictionary _varNameDic = new UniqueNameDictionary();

        public EntityInfo(EntityMetadata entityMetadata, List<ActionInfo> actionList, string uniqueDisplayName)
        {
            // Add the LogicalName of the entity to the dictionary - prevent Attribtues named equal to the wrapping type
            _varNameDic.Add($"entity.{entityMetadata.LogicalName}", uniqueDisplayName);

            UniqueDisplayName = uniqueDisplayName;
            TypeCode = entityMetadata.ObjectTypeCode;
            LogicalName = entityMetadata.LogicalName;

            ActionList = actionList;
            OptionSetList = ParseOptionSets(entityMetadata);
            AttributeList = ParseAttributes(entityMetadata);

            PrimaryNameAttribute = entityMetadata.PrimaryNameAttribute != null ? AttributeList.Single(a => a.LogicalName == entityMetadata.PrimaryNameAttribute) : null;
            PrimaryIdAttribute = entityMetadata.PrimaryIdAttribute != null ? AttributeList.Single(a => a.LogicalName == entityMetadata.PrimaryIdAttribute) : null;
        }

        public void AddFormInfo(Entity systemForm)
        {
            if (!FormList.Any(f => f.Name == systemForm["name"] as string))
            {
                FormList.Add(new FormInfo(systemForm));
            }
        }

        public void AddManyToManyRelationInfo(EntityInfo relatedEntityInfo, ManyToManyRelationshipMetadata metadata)
        {
            var relationInfo = new ManyToManyRelationInfo(
                relatedEntityInfo,
                metadata.IntersectEntityName,
                metadata.SchemaName,
                _varNameDic.GetUniqueName(metadata.SchemaName, metadata.SchemaName),
                _varNameDic.GetUniqueName(metadata.IntersectEntityName, metadata.IntersectEntityName))
            {
                Entity1Attribute = metadata.Entity1IntersectAttribute,
                Entity1LogicalName = metadata.Entity1LogicalName,
                Entity2Attribute = metadata.Entity2IntersectAttribute,
                Entity2LogicalName = metadata.Entity2LogicalName
            };
            ManyToManyRelationList.Add(relationInfo);
        }

        public void AddOneToManyRelationInfo(EntityInfo relatedEntityInfo, OneToManyRelationshipMetadata metadata)
        {
            var relationInfo = new OneToManyRelationInfo(
                relatedEntityInfo,
                metadata.SchemaName,
                _varNameDic.GetUniqueName(metadata.SchemaName, metadata.SchemaName))
            {
                RelatedEntityAttributeName = metadata.ReferencingAttribute
            };
            OneToManyRelationList.Add(relationInfo);
        }

        private List<OptionSetInfo> ParseOptionSets(EntityMetadata entityMetadata)
        {
            return entityMetadata.Attributes
                            .Where(a =>
                                a.AttributeType == AttributeTypeCode.Picklist ||
                                a.AttributeType == AttributeTypeCode.State ||
                                a.AttributeType == AttributeTypeCode.Status ||
                                a is MultiSelectPicklistAttributeMetadata)
                            .DistinctBy(attr => ((EnumAttributeMetadata)attr).OptionSet.Name)
                            .OrderBy(attr => ((EnumAttributeMetadata)attr).OptionSet.Name)
                            .Select(osAttr => new OptionSetInfo(((EnumAttributeMetadata)osAttr).OptionSet, _varNameDic.GetUniqueName(((EnumAttributeMetadata)osAttr).OptionSet)))
                            .ToList();
        }

        private List<AttributeInfo> ParseAttributes(EntityMetadata entityMetadata)
        {
            var parsedAttributes = new List<AttributeInfo>();
            foreach (var attributeMetadata in entityMetadata.Attributes.OrderBy(a => a.LogicalName))
            {
                switch (attributeMetadata.AttributeType)
                {
                    case AttributeTypeCode.ManagedProperty:
                        break;
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                    case AttributeTypeCode.Picklist:
                        var optionSet = OptionSetList.Single(os => os.LogicalName == ((EnumAttributeMetadata)attributeMetadata).OptionSet.Name);
                        parsedAttributes.Add(new AttributeInfo(attributeMetadata, _varNameDic.GetUniqueName(attributeMetadata), optionSet.UniqueDisplayName));
                        break;
                    case AttributeTypeCode.Virtual:
                        if (attributeMetadata is MultiSelectPicklistAttributeMetadata multiSelect)
                        {
                            var multiOptionSet = OptionSetList.Single(os => os.LogicalName == multiSelect.OptionSet.Name);
                            parsedAttributes.Add(new AttributeInfo(attributeMetadata, _varNameDic.GetUniqueName(attributeMetadata), multiOptionSet.UniqueDisplayName));
                        }
                        break;
                    case AttributeTypeCode.Money:
                        if (!attributeMetadata.LogicalName.EndsWith("_base")) // Don't add the readonly base money field
                            parsedAttributes.Add(new AttributeInfo(attributeMetadata, _varNameDic.GetUniqueName(attributeMetadata)));
                        break;
                    case AttributeTypeCode.Uniqueidentifier:
                        parsedAttributes.Add(new AttributeInfo(attributeMetadata, _varNameDic.GetUniqueName(attributeMetadata, "Id")));
                        break;
                    default:
                        parsedAttributes.Add(new AttributeInfo(attributeMetadata, _varNameDic.GetUniqueName(attributeMetadata)));
                        break;
                }
            }
            return parsedAttributes;
        }

        public int? TypeCode { get; }
        public string LogicalName { get; }
        public string UniqueDisplayName { get; }
        public AttributeInfo PrimaryNameAttribute { get; }
        public AttributeInfo PrimaryIdAttribute { get; }

        public List<AttributeInfo> AttributeList { get; }
        public List<OptionSetInfo> OptionSetList { get; }
        public List<OneToManyRelationInfo> OneToManyRelationList { get; } = new List<OneToManyRelationInfo>();
        public List<ManyToManyRelationInfo> ManyToManyRelationList { get; } = new List<ManyToManyRelationInfo>();
        public List<ActionInfo> ActionList { get; } = new List<ActionInfo>();

        public List<FormInfo> FormList { get; } = new List<FormInfo>();
    }
}
