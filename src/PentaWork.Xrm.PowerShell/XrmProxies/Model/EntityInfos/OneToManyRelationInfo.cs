namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class OneToManyRelationInfo
    {
        public OneToManyRelationInfo(EntityInfo relatedEntityInfo, string schemaName, string uniqueDisplayName)
        {
            SchemaName = schemaName;
            RelatedEntityInfo = relatedEntityInfo;
            UniqueDisplayName = uniqueDisplayName;
        }

        public string UniqueDisplayName { get; }
        public string SchemaName { get; }
        public EntityInfo RelatedEntityInfo { get; }

        public string RelatedEntityAttributeName { get; set; }
    }
}
