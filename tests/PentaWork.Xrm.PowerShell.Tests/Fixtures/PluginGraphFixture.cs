using PentaWork.Xrm.PluginGraph.Model.GraphObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PowerShell.Tests.Fixtures
{
    /// <summary>
    /// Minimal EntityGraph covering MainTemplate.tt's stage/sync-async/rank nesting and the
    /// FilteringAttributes/AsyncAutoDelete branches. Plugin is left null on both steps to skip
    /// the ApiCalls block, which needs a much heavier IL-analysis fixture out of scope here.
    /// </summary>
    internal static class PluginGraphFixture
    {
        public static EntityGraph BuildEntityGraph()
        {
            var graph = new EntityGraph("test_entity");

            graph.Add(new PluginStepInfo
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "Test.Plugins.PreOperationPlugin",
                SdkMessage = "Update",
                Stage = Stage.PreOperation,
                Async = false,
                Rank = 1,
                FilteringAttributes = new List<string> { "name", "revenue" }
            });

            graph.Add(new PluginStepInfo
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "Test.Plugins.PostOperationAsyncPlugin",
                SdkMessage = "Update",
                Stage = Stage.PostOperation,
                Async = true,
                AsyncAutoDelete = true,
                Rank = null
            });

            return graph;
        }
    }
}
