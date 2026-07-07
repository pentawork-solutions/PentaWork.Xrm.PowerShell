using PentaWork.Xrm.PowerShell.Tests.Fixtures;
using PentaWork.Xrm.PowerShell.XrmProxies.Model;
using ScribanTemplateRenderer = PentaWork.Xrm.PowerShell.XrmProxies.Templates.ScribanTemplateRenderer;
using PluginGraphScribanTemplateRenderer = PentaWork.Xrm.PluginGraph.Templates.ScribanTemplateRenderer;

namespace PentaWork.Xrm.PowerShell.Tests
{
    /// <summary>
    /// Asserts the ported Scriban templates produce the same content as the golden files
    /// captured from the original T4 templates (see GoldenFileCapture).
    /// </summary>
    [TestClass]
    public class TemplateGoldenFileTests : GoldenFileTestBase
    {
        private const string ProxyNamespace = "Test.Proxies";
        private const string FakeNamespace = "Test.Fakes";

        private static EntityInfo GetMainEntity() =>
            MetadataFixture.BuildEntityInfoList().Single(e => e.LogicalName == "test_entity");

        [TestMethod]
        public void Attributes_MatchesGoldenFile()
        {
            var result = ScribanTemplateRenderer.Render("CSharp.Attributes", new { ProxyNamespace });
            AssertMatchesGoldenFile("Attributes.cs.golden", result);
        }

        [TestMethod]
        public void BaseProxy_MatchesGoldenFile()
        {
            var result = ScribanTemplateRenderer.Render("CSharp.BaseProxy", new { ProxyNamespace });
            AssertMatchesGoldenFile("BaseProxy.cs.golden", result);
        }

        [TestMethod]
        public void EnumExtensions_MatchesGoldenFile()
        {
            var result = ScribanTemplateRenderer.Render("CSharp.EnumExtensions", new { ProxyNamespace });
            AssertMatchesGoldenFile("EnumExtensions.cs.golden", result);
        }

        [TestMethod]
        public void AssemblyInfoAddition_MatchesGoldenFile()
        {
            var result = ScribanTemplateRenderer.Render("CSharp.AssemblyInfoAddition", new { });
            AssertMatchesGoldenFile("AssemblyInfoAddition.cs.golden", result);
        }

        [TestMethod]
        public void OptionSet_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            foreach (var optionSetInfo in entityInfo.OptionSetList)
            {
                var result = ScribanTemplateRenderer.Render("CSharp.OptionSet", new { OptionSetInfo = optionSetInfo });
                AssertMatchesGoldenFile($"OptionSet.{optionSetInfo.UniqueDisplayName}.cs.golden", result);
            }
        }

        [TestMethod]
        public void ProxyClass_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            var optionSetEnumsCs = string.Join(Environment.NewLine,
                entityInfo.OptionSetList.Select(os => ScribanTemplateRenderer.Render("CSharp.OptionSet", new { OptionSetInfo = os })));
            var result = ScribanTemplateRenderer.Render("CSharp.ProxyClass", new { EntityInfo = entityInfo, ProxyNamespace, UseBaseProxy = true, OptionSetEnumsCs = optionSetEnumsCs });
            AssertMatchesGoldenFile("ProxyClass.cs.golden", result);
        }

        [TestMethod]
        public void Fake_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            var result = ScribanTemplateRenderer.Render("CSharp.Fake", new { EntityInfo = entityInfo, ProxyNamespace, FakeNamespace });
            AssertMatchesGoldenFile("Fake.cs.golden", result);
        }

        [TestMethod]
        public void RelationProxyClass_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            var relation = entityInfo.ManyToManyRelationList.Single();
            var result = ScribanTemplateRenderer.Render("CSharp.RelationProxyClass", new { RelationClassInfo = relation, ProxyNamespace, UseBaseProxy = true });
            AssertMatchesGoldenFile("RelationProxyClass.cs.golden", result);
        }

        [TestMethod]
        public void ProxyTypes_MatchesGoldenFile()
        {
            var result = ScribanTemplateRenderer.Render("Javascript.ProxyTypes", new { });
            AssertMatchesGoldenFile("ProxyTypes.ts.golden", result);
        }

        [TestMethod]
        public void AttributeJS_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            var result = ScribanTemplateRenderer.Render("Javascript.AttributeJS", new { EntityInfo = entityInfo });
            AssertMatchesGoldenFile("AttributeJS.ts.golden", result);
        }

        [TestMethod]
        public void OptionSetJS_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            foreach (var optionSetInfo in entityInfo.OptionSetList)
            {
                var result = ScribanTemplateRenderer.Render("Javascript.OptionSetJS", new { OptionSetInfo = optionSetInfo });
                AssertMatchesGoldenFile($"OptionSetJS.{optionSetInfo.UniqueDisplayName}.ts.golden", result);
            }
        }

        [TestMethod]
        public void ProxyClassJS_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            var optionSetEnumsJs = string.Join(Environment.NewLine,
                entityInfo.OptionSetList.Select(os => ScribanTemplateRenderer.Render("Javascript.OptionSetJS", new { OptionSetInfo = os })));
            var result = ScribanTemplateRenderer.Render("Javascript.ProxyClassJS", new { EntityInfo = entityInfo, OptionSetEnumsJs = optionSetEnumsJs });
            AssertMatchesGoldenFile("ProxyClassJS.ts.golden", result);
        }

        [TestMethod]
        public void FormInfosJS_MatchesGoldenFile()
        {
            var entityInfo = GetMainEntity();
            var result = ScribanTemplateRenderer.Render("Javascript.FormInfosJS", new { EntityInfo = entityInfo });
            AssertMatchesGoldenFile("FormInfosJS.ts.golden", result);
        }

        [TestMethod]
        public void MainTemplate_MatchesGoldenFile()
        {
            var entityGraph = PluginGraphFixture.BuildEntityGraph();
            var result = PluginGraphScribanTemplateRenderer.Render("MainTemplate", new { EntityGraph = entityGraph });
            AssertMatchesGoldenFile("MainTemplate.md.golden", result);
        }
    }
}
