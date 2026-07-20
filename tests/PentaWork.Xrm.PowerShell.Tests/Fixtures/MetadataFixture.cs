using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using PentaWork.Xrm.PowerShell.XrmProxies.Model;

namespace PentaWork.Xrm.PowerShell.Tests.Fixtures
{
    /// <summary>
    /// Hand-built CRM metadata covering every branch ProxyClass.tt/Fake.tt switch on
    /// (Money, Decimal, Picklist, multi-select Picklist, Lookup, primary id, primary name,
    /// a 1:N relation, an N:N relation, an Action, a form with two tabs), used to snapshot
    /// T4 output before the Scriban migration and assert Scriban output matches afterward.
    /// </summary>
    internal static class MetadataFixture
    {
        public static EntityInfoList BuildEntityInfoList()
        {
            var mainEntity = BuildMainEntityMetadata();
            var relatedEntity = BuildRelatedEntityMetadata();

            AddOneToManyRelationship(mainEntity, relatedEntity);
            AddManyToManyRelationship(mainEntity, relatedEntity);

            var entityMetadataList = new List<EntityMetadata> { mainEntity, relatedEntity };
            var systemForms = new List<Entity> { BuildSystemForm() };
            var actions = new List<ActionInfo> { new ActionInfo("DoSomething", "new_DoSomething", "test_entity") };

            return new EntityInfoList(entityMetadataList, systemForms, actions);
        }

        private static EntityMetadata BuildMainEntityMetadata()
        {
            var entity = new EntityMetadata();
            MetadataReflectionHelper.SetProperty(entity, "LogicalName", "test_entity");
            MetadataReflectionHelper.SetProperty(entity, "ObjectTypeCode", 10001);
            MetadataReflectionHelper.SetProperty(entity, "PrimaryIdAttribute", "test_entityid");
            MetadataReflectionHelper.SetProperty(entity, "PrimaryNameAttribute", "name");
            entity.DisplayName = new Label("Test Entity", 1033);

            var id = new UniqueIdentifierAttributeMetadata("test_entityid") { LogicalName = "test_entityid", DisplayName = new Label("Test Entity", 1033) };
            var name = new StringAttributeMetadata("name") { LogicalName = "name", DisplayName = new Label("Name", 1033) };

            var revenue = new MoneyAttributeMetadata
            {
                LogicalName = "revenue",
                DisplayName = new Label("Revenue", 1033),
                Precision = 2,
                MaxValue = 1000000000,
                MinValue = 0
            };

            var rating = new DecimalAttributeMetadata
            {
                LogicalName = "rating",
                DisplayName = new Label("Rating", 1033),
                Precision = 2,
                MaxValue = 100,
                MinValue = 0
            };

            var priorityOptionSet = new OptionSetMetadata(new OptionMetadataCollection(new List<OptionMetadata>
            {
                new OptionMetadata(new Label("Low", 1033), 1),
                new OptionMetadata(new Label("Medium", 1033), 2),
                new OptionMetadata(new Label("High", 1033), 3)
            }))
            { Name = "test_entity_prioritycode", IsGlobal = false, DisplayName = new Label("Priority", 1033) };
            var priority = new PicklistAttributeMetadata
            {
                LogicalName = "prioritycode",
                DisplayName = new Label("Priority", 1033),
                OptionSet = priorityOptionSet
            };

            var categoriesOptionSet = new OptionSetMetadata(new OptionMetadataCollection(new List<OptionMetadata>
            {
                new OptionMetadata(new Label("Alpha", 1033), 1),
                new OptionMetadata(new Label("Beta", 1033), 2)
            }))
            { Name = "test_entity_categories", IsGlobal = false, DisplayName = new Label("Categories", 1033) };
            var categories = new MultiSelectPicklistAttributeMetadata
            {
                LogicalName = "categories",
                DisplayName = new Label("Categories", 1033),
                OptionSet = categoriesOptionSet
            };

            var parent = new LookupAttributeMetadata
            {
                LogicalName = "parentid",
                DisplayName = new Label("Parent", 1033),
                Targets = new[] { "related_entity" }
            };

            MetadataReflectionHelper.SetProperty(entity, "Attributes", new AttributeMetadata[]
            {
                id, name, revenue, rating, priority, categories, parent
            });

            return entity;
        }

        private static EntityMetadata BuildRelatedEntityMetadata()
        {
            var entity = new EntityMetadata();
            MetadataReflectionHelper.SetProperty(entity, "LogicalName", "related_entity");
            MetadataReflectionHelper.SetProperty(entity, "ObjectTypeCode", 10002);
            MetadataReflectionHelper.SetProperty(entity, "PrimaryIdAttribute", "related_entityid");
            MetadataReflectionHelper.SetProperty(entity, "PrimaryNameAttribute", "name");
            entity.DisplayName = new Label("Related Entity", 1033);

            var id = new UniqueIdentifierAttributeMetadata("related_entityid") { LogicalName = "related_entityid", DisplayName = new Label("Related Entity", 1033) };
            var name = new StringAttributeMetadata("name") { LogicalName = "name", DisplayName = new Label("Name", 1033) };

            MetadataReflectionHelper.SetProperty(entity, "Attributes", new AttributeMetadata[] { id, name });

            return entity;
        }

        private static void AddOneToManyRelationship(EntityMetadata mainEntity, EntityMetadata relatedEntity)
        {
            var oneToMany = new OneToManyRelationshipMetadata
            {
                SchemaName = "test_entity_related_entity",
                ReferencingEntity = "related_entity",
                ReferencingAttribute = "test_entityid_parent"
            };
            MetadataReflectionHelper.SetProperty(mainEntity, "OneToManyRelationships", new[] { oneToMany });
        }

        private static void AddManyToManyRelationship(EntityMetadata mainEntity, EntityMetadata relatedEntity)
        {
            var manyToMany = new ManyToManyRelationshipMetadata
            {
                SchemaName = "test_entity_related_entity_assoc",
                IntersectEntityName = "test_entity_related_entity_assoc",
                Entity1LogicalName = "test_entity",
                Entity2LogicalName = "related_entity",
                Entity1IntersectAttribute = "test_entityid",
                Entity2IntersectAttribute = "related_entityid"
            };
            MetadataReflectionHelper.SetProperty(mainEntity, "ManyToManyRelationships", new[] { manyToMany });
            MetadataReflectionHelper.SetProperty(relatedEntity, "ManyToManyRelationships", System.Array.Empty<ManyToManyRelationshipMetadata>());
            MetadataReflectionHelper.SetProperty(relatedEntity, "OneToManyRelationships", System.Array.Empty<OneToManyRelationshipMetadata>());
        }

        private static Entity BuildSystemForm()
        {
            var form = new Entity("systemform");
            form["name"] = "Information";
            form["objecttypecode"] = "test_entity";
            form["formxml"] = "<form><tabs><tab name=\"General\" /><tab name=\"Details\" /></tabs></form>";
            return form;
        }
    }
}
