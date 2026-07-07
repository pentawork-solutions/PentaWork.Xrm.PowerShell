using System.IO;
using System.Reflection;
using PentaWork.Xrm.PowerShell;
using PentaWork.Xrm.PowerShell.Tests.Fixtures;

namespace PentaWork.Xrm.PowerShell.Tests
{
    /// <summary>
    /// Exercises the real Get-XrmProxies file-generation path (private methods invoked via
    /// reflection, since ProcessRecord() requires a live CRM connection for metadata retrieval
    /// which isn't touched by the Scriban migration). Confirms the cmdlet's own wiring of
    /// ScribanTemplateRenderer + File.WriteAllText + folder creation works end-to-end, not just
    /// the renderer in isolation like TemplateGoldenFileTests does.
    /// </summary>
    [TestClass]
    public class GetXrmProxiesSmokeTest
    {
        [TestMethod]
        public void GenerateAll_ProducesExpectedFileTree()
        {
            var outputPath = Path.Combine(Path.GetTempPath(), "PentaWorkSmokeTest_" + System.Guid.NewGuid());
            Directory.CreateDirectory(outputPath);
            try
            {
                var cmdlet = new GetXrmProxies
                {
                    ProxyNamespace = "Smoke.Proxies",
                    FakeNamespace = "Smoke.Fakes",
                    OutputPath = new DirectoryInfo(outputPath),
                    UseBaseProxy = true,
                    CommandRuntime = new FakeCommandRuntime()
                };

                var entityInfoList = MetadataFixture.BuildEntityInfoList();

                var cmdletType = typeof(GetXrmProxies);
                var ensureFolder = cmdletType.GetMethod("EnsureFolder", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var generateBaseClasses = cmdletType.GetMethod("GenerateBaseClasses", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var generateAllCSharp = cmdletType.GetMethod("GenerateAllCSharp", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var generateAllJavascript = cmdletType.GetMethod("GenerateAllJavascript", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var csOutputPath = (string)cmdletType.GetProperty("CSOutputPath")!.GetValue(cmdlet)!;
                var tsOutputPath = (string)cmdletType.GetProperty("TSOutputPath")!.GetValue(cmdlet)!;

                ensureFolder.Invoke(cmdlet, new object[] { outputPath });
                ensureFolder.Invoke(cmdlet, new object[] { csOutputPath });
                ensureFolder.Invoke(cmdlet, new object[] { tsOutputPath });

                generateBaseClasses.Invoke(cmdlet, null);
                generateAllCSharp.Invoke(cmdlet, new object[] { entityInfoList });
                generateAllJavascript.Invoke(cmdlet, new object[] { entityInfoList });

                // Base classes
                Assert.IsTrue(File.Exists(Path.Combine(csOutputPath, "BaseProxy.cs")));
                Assert.IsTrue(File.Exists(Path.Combine(csOutputPath, "Attributes.cs")));
                Assert.IsTrue(File.Exists(Path.Combine(csOutputPath, "AssemblyInfoAddition.cs")));
                Assert.IsTrue(File.Exists(Path.Combine(csOutputPath, "Extensions", "EnumExtensions.cs")));

                // Per-entity C# proxy + fake classes
                Assert.IsTrue(File.Exists(Path.Combine(csOutputPath, "Entities", "TestEntity.cs")));
                Assert.IsTrue(File.Exists(Path.Combine(csOutputPath, "Entities", "RelatedEntity.cs")));
                Assert.IsTrue(File.Exists(Path.Combine(outputPath, "Fake", "TestEntity.cs")));
                Assert.IsTrue(File.Exists(Path.Combine(csOutputPath, "Relations", "TestEntityRelatedEntityAssoc.cs")));

                // TypeScript output
                Assert.IsTrue(File.Exists(Path.Combine(tsOutputPath, "ProxyTypes.ts")));
                Assert.IsTrue(File.Exists(Path.Combine(tsOutputPath, "Entities", "TestEntity.ts")));
                Assert.IsTrue(File.Exists(Path.Combine(tsOutputPath, "Attributes", "TestEntity.ts")));
                Assert.IsTrue(File.Exists(Path.Combine(tsOutputPath, "FormInfos", "TestEntity.ts")));

                var proxyClassContent = File.ReadAllText(Path.Combine(csOutputPath, "Entities", "TestEntity.cs"));
                StringAssert.Contains(proxyClassContent, "public enum ePriority");
                StringAssert.Contains(proxyClassContent, "public decimal? Revenue");
                StringAssert.Contains(proxyClassContent, "public override Guid Id");
            }
            finally
            {
                Directory.Delete(outputPath, recursive: true);
            }
        }
    }
}
