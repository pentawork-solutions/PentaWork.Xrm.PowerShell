using PentaWork.Xrm.PluginGraph.Model.GraphObjects;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PowerShell.Tests.Fixtures
{
    /// <summary>
    /// A small two-entity system covering MainTemplate.sbn's stage/sync-async/rank nesting,
    /// the FilteringAttributes/AsyncAutoDelete branches, and the trigger-linking heuristics:
    /// - "account"'s PreOperation step updates "account" itself while targeted -> self-recursion.
    /// - "account"'s PostOperation async step creates a "task" -> cross-entity trigger link to
    ///   "task"'s Create step (and the reverse "triggered by" link on that step).
    /// </summary>
    internal static class PluginGraphFixture
    {
        public static EntityGraphList BuildEntityGraphList()
        {
            var accountUpdatePreOp = new PluginStepInfo
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "Test.Plugins.AccountUpdatePreOpPlugin",
                SdkMessage = "Update",
                PrimaryEntityName = "account",
                Stage = Stage.PreOperation,
                Async = false,
                Rank = 1,
                FilteringAttributes = new List<string> { "name", "revenue" },
                Plugin = new PluginInfo { TypeName = "Test.Plugins.AccountUpdatePreOpPlugin" }
            };

            var accountUpdatePostOpAsync = new PluginStepInfo
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "Test.Plugins.AccountUpdatePostOpAsyncPlugin",
                SdkMessage = "Update",
                PrimaryEntityName = "account",
                Stage = Stage.PostOperation,
                Async = true,
                AsyncAutoDelete = true,
                Plugin = new PluginInfo { TypeName = "Test.Plugins.AccountUpdatePostOpAsyncPlugin" }
            };

            var taskCreatePlugin = new PluginStepInfo
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "Test.Plugins.TaskCreatePlugin",
                SdkMessage = "Create",
                PrimaryEntityName = "task",
                Stage = Stage.PostOperation,
                Async = false,
                Rank = 1,
                Plugin = new PluginInfo { TypeName = "Test.Plugins.TaskCreatePlugin" }
            };

            var apiCalls = new Dictionary<string, List<XrmApiCall>>
            {
                [accountUpdatePreOp.Plugin.TypeName] = new List<XrmApiCall>
                {
                    new XrmApiCall
                    {
                        Message = "update",
                        EntityInfo = new EntityObj { LogicalName = "account", IsTarget = true, UsedFields = new List<string> { "name" } },
                        IsExecuted = true
                    }
                },
                [accountUpdatePostOpAsync.Plugin.TypeName] = new List<XrmApiCall>
                {
                    new XrmApiCall
                    {
                        Message = "create",
                        EntityInfo = new EntityObj { LogicalName = "task" },
                        IsExecuted = true
                    }
                },
                [taskCreatePlugin.Plugin.TypeName] = new List<XrmApiCall>()
            };

            var entityGraphList = new EntityGraphList(apiCalls);
            entityGraphList.Add(accountUpdatePreOp);
            entityGraphList.Add(accountUpdatePostOpAsync);
            entityGraphList.Add(taskCreatePlugin);
            entityGraphList.LinkTriggers();

            return entityGraphList;
        }
    }
}
