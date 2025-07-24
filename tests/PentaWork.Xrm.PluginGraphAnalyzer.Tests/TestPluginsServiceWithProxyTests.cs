using PentaWork.Xrm.PluginGraph;

namespace PentaWork.Xrm.PluginGraphTests
{
    [TestClass]
    public sealed class TestPluginsServiceWithProxyTests
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
        public void ShouldAnalyseServiceCreateWithProxiesSuccessfully()
        {
            // Arrange
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceWithProxyCreate"]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseServiceCreateInMethodWithProxiesSuccessfully()
        {
            // Arrange
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceInMethodWithProxyCreate"]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseServiceCreateInSideMethodWithProxiesSuccessfully()
        {
            // Arrange
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceInSideMethodWithProxyCreate"]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseServiceCreateDirectWithSideProxiesSuccessfully()
        {
            // Arrange
            // Act
            var apiCalls = _pluginGraphAnalyzer.Analyze([
                "PentaWork.Xrm.Tests.Plugins.TestPluginServiceDirectWithSideProxyCreate"]);
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }
    }
}
