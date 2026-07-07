using PentaWork.Xrm.PluginGraph.Model.GraphObjects;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;
using PentaWork.Xrm.PowerShell.Tests.Fixtures;

namespace PentaWork.Xrm.PowerShell.Tests
{
    /// <summary>
    /// Covers the system-wide overview page: the file/step summary, the table of contents, and the
    /// entity-level (not step-level) relationship diagram that replaced the per-entity diagram.
    /// </summary>
    [TestClass]
    public class SystemOverviewTests
    {
        [TestMethod]
        public void Summary_ReportsTotalsAcrossAllEntities()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var overview = new SystemOverview(entityGraphList);

            Assert.AreEqual(2, overview.TotalEntities); // "account" and "task"
            Assert.AreEqual(3, overview.TotalSteps);
            Assert.AreEqual(2, overview.TotalSyncSteps);
            Assert.AreEqual(1, overview.TotalAsyncSteps);
            Assert.AreEqual(2, overview.TotalApiCalls);
        }

        [TestMethod]
        public void TableOfContents_ListsEveryEntityWithItsFileNameAndStepCount()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var overview = new SystemOverview(entityGraphList);

            var accountRow = overview.Entities.Single(e => e.EntityName == "account");
            Assert.AreEqual("account.md", accountRow.FileName);
            Assert.AreEqual(2, accountRow.TotalSteps);
            Assert.IsTrue(accountRow.HasSelfRecursionWarnings);

            var taskRow = overview.Entities.Single(e => e.EntityName == "task");
            Assert.AreEqual("task.md", taskRow.FileName);
            Assert.AreEqual(1, taskRow.TotalSteps);
            Assert.IsFalse(taskRow.HasSelfRecursionWarnings);
        }

        [TestMethod]
        public void Edges_AggregatesStepLevelTriggersDownToOneEdgePerEntityPair()
        {
            // The fixture's two "account" Update steps trigger each other (the PreOp step's own
            // Update-on-account call matches the PostOp step's unconditional Update registration),
            // in addition to the PostOp step's Create-on-task call triggering "task"'s Create step.
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var overview = new SystemOverview(entityGraphList);

            Assert.AreEqual(2, overview.Edges.Count);

            var crossEntityEdge = overview.Edges.Single(e => e.ToEntity == "task");
            Assert.AreEqual("account", crossEntityEdge.FromEntity);
            Assert.AreEqual("create", crossEntityEdge.MessagesLabel);
            Assert.IsFalse(crossEntityEdge.IsSelfLoop);

            var selfLoopEdge = overview.Edges.Single(e => e.IsSelfLoop);
            Assert.AreEqual("account", selfLoopEdge.FromEntity);
            Assert.AreEqual("update", selfLoopEdge.MessagesLabel);

            CollectionAssert.Contains(overview.DiagramNodes.Select(n => n.EntityName).ToList(), "account");
            CollectionAssert.Contains(overview.DiagramNodes.Select(n => n.EntityName).ToList(), "task");
        }

        [TestMethod]
        public void ToMarkdown_RendersSummaryTocAndDiagram()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var markdown = new SystemOverview(entityGraphList).ToMarkdown();

            StringAssert.Contains(markdown, "# Plugin Graph Overview");
            StringAssert.Contains(markdown, "**Entities**: 2");
            StringAssert.Contains(markdown, "| [account](account.md) ⚠ | 2 | 1 / 1 |");
            StringAssert.Contains(markdown, "| [task](task.md) | 1 | 1 / 0 |");
            StringAssert.Contains(markdown, "```mermaid");
            StringAssert.Contains(markdown, "e_account[\"account\"]");
            StringAssert.Contains(markdown, "e_task[\"task\"]");
            StringAssert.Contains(markdown, "e_account -->|\"create\"| e_task");
            StringAssert.Contains(markdown, "e_account -.->|\"update\"| e_account");
            StringAssert.Contains(markdown, "class e_account selfLoop");
        }

        [TestMethod]
        public void ToMarkdown_NoTriggerRelationships_OmitsDiagramWithoutError()
        {
            var step = new PluginStepInfo
            {
                Id = Guid.NewGuid(),
                Name = "Isolated",
                SdkMessage = "Create",
                PrimaryEntityName = "account",
                Stage = Stage.PostOperation,
                Plugin = new PluginInfo { TypeName = "Isolated" }
            };
            var apiCalls = new Dictionary<string, List<XrmApiCall>> { ["Isolated"] = new List<XrmApiCall>() };

            var entityGraphList = new EntityGraphList(apiCalls);
            entityGraphList.Add(step);
            entityGraphList.LinkTriggers();

            var markdown = new SystemOverview(entityGraphList).ToMarkdown();

            StringAssert.DoesNotMatch(markdown, new System.Text.RegularExpressions.Regex(@"```mermaid"));
            StringAssert.Contains(markdown, "No cross-entity trigger relationships detected.");
        }
    }
}
