using PentaWork.Xrm.PluginGraph;

namespace PentaWork.Xrm.PluginGraphTests
{
    [TestClass]
    public sealed class TestPluginsServiceContextWithProxyTests
    {
        private PluginGraphAnalyzer _pluginGraphAnalyzer;

        [TestInitialize]
        public void Initialize()
        {
            var testAssemblyPath = GetType().Assembly.Location;
            var pluginAssemblyPath = Path.Combine(testAssemblyPath, "..\\..\\..\\..\\..\\PentaWork.Xrm.Tests.Plugins\\bin\\Debug\\net462");
            _pluginGraphAnalyzer = new PluginGraphAnalyzer(pluginAssemblyPath);
        }

        [TestMethod]
        public void ShouldAnalyseServiceContextCreateWithProxiesSuccessfully()
        {
            // Arrange
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyCreate"]);
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
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyUpdate"]);
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
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyDelete"]);
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
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyDeleteNoSave"]);
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
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyNoSaveButOtherContext"]);
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
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceContextWithProxyClearsChangesAndAddsSomeAgain"]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsTrue(pluginApiCalls.Count == 2);
            Assert.IsTrue(pluginApiCalls.Any(p => p.IsExecuted));
            Assert.IsTrue(pluginApiCalls.Any(p => !p.IsExecuted));
        }
    }
}
