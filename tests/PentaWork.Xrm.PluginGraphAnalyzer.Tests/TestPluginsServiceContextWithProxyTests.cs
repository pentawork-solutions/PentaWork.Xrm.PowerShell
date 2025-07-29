using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraphTests
{
    [TestClass]
    public sealed class TestPluginsServiceContextWithProxyTests
    {
        private Dictionary<Guid, PluginModuleList> _moduleList;
        private readonly PluginGraphAnalyzer _pluginGraphAnalyzer = new PluginGraphAnalyzer();
        private readonly PluginStepInfo _pluginStepInfo = new PluginStepInfo
        {
            Plugin = new PluginInfo
            {
                AssemblyInfo = new AssemblyInfo
                {
                    Name = "PentaWork.Xrm.Tests.Plugins",
                },
                PackageInfo = new PackageInfo
                {
                    Id = new Guid("f305f42a-c37a-487a-a4b0-521b946315b6")
                }
            }
        };

        [TestInitialize]
        public void Initialize()
        {
            var testAssemblyPath = GetType().Assembly.Location;
            var pluginAssemblyPath = Path.Combine(testAssemblyPath, "..\\..\\..\\..\\..\\PentaWork.Xrm.Tests.Plugins\\bin\\Debug\\net462");
            var assemblyList = Directory.GetFiles(pluginAssemblyPath, "*.dll", SearchOption.AllDirectories);

            var moduleList = new PluginModuleList();
            foreach (var assemblyFile in assemblyList)
            {
                moduleList.Add(ModuleDefMD.Load(assemblyFile));
            }
            _moduleList = new Dictionary<Guid, PluginModuleList>
            {
                { new Guid("f305f42a-c37a-487a-a4b0-521b946315b6"), moduleList }
            };
        }

        [TestMethod]
        public void ShouldAnalyseServiceContextCreateWithProxiesSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyCreate";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.IsTrue(pluginApiCalls.FirstOrDefault()?.IsExecuted);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseServiceContextUpdateWithProxiesSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyUpdate";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.IsTrue(pluginApiCalls.FirstOrDefault()?.IsExecuted);
            Assert.AreEqual("update", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseServiceContextDeleteWithProxiesSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyDelete";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.IsTrue(pluginApiCalls.FirstOrDefault()?.IsExecuted);
            Assert.AreEqual("delete", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldRecognizeNotExecutedCallsSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyDeleteNoSave";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.IsFalse(pluginApiCalls.FirstOrDefault()?.IsExecuted);
            Assert.AreEqual("delete", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldRecognizeNotExecutedCallsButOtherContextCallsSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyNoSaveButOtherContext";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsTrue(pluginApiCalls.Count == 2);
            Assert.IsTrue(pluginApiCalls.Any(p => p.IsExecuted));
            Assert.IsTrue(pluginApiCalls.Any(p => !p.IsExecuted));
        }

        [TestMethod]
        public void ShouldRecognizeAttachedAndClearedCallsSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyClearsChangesAndAddsSomeAgain";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsTrue(pluginApiCalls.Count == 2);
            Assert.IsTrue(pluginApiCalls.Any(p => p.IsExecuted));
            Assert.IsTrue(pluginApiCalls.Any(p => !p.IsExecuted));
        }
    }
}
