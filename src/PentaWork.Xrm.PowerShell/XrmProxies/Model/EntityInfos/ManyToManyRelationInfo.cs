namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{ 
    public class ManyToManyRelationInfo
    {
        private readonly UniqueNameDictionary _attrNameDic = new UniqueNameDictionary();

        public ManyToManyRelationInfo(EntityInfo relatedEntityInfo, string intersectEntityName, string schemaName, string uniqueDisplayName, string uniqueIntersectDisplayName)
        {
            IntersectEntityName = intersectEntityName;
            SchemaName = schemaName;
            RelatedEntityInfo = relatedEntityInfo;
            UniqueDisplayName = uniqueDisplayName;
            UniqueIntersectDisplayName = uniqueIntersectDisplayName;
        }

        public string UniqueDisplayName { get; }
        public string UniqueIntersectDisplayName { get; }
        public string SchemaName { get; }
        public string IntersectEntityName { get; }
        public EntityInfo RelatedEntityInfo { get; }

        public string Entity1LogicalName { get; set; }
        public string Entity2LogicalName { get; set; }

        public string _entity1Attribute;
        public string Entity1Attribute
        {
            get => _entity1Attribute;
            set
            {
                _entity1Attribute = value;
                UniqueEntity1AttributeName = _attrNameDic.GetUniqueName(_entity1Attribute, _entity1Attribute, string.Empty, "Id");
            }
        }

        public string _entity2Attribute;
        public string Entity2Attribute
        {
            get => _entity2Attribute;
            set
            {
                _entity2Attribute = value;
                UniqueEntity2AttributeName = _attrNameDic.GetUniqueName(_entity2Attribute, _entity2Attribute, string.Empty, "Id");
            }
        }

        public string UniqueEntity1AttributeName { get; private set; }
        public string UniqueEntity2AttributeName { get; private set; }
    }
}
