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
            Assert.AreEqual("update", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }
    }
}
