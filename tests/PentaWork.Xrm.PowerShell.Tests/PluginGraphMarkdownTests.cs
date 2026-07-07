using PentaWork.Xrm.PluginGraph.Model.GraphObjects;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;
using PentaWork.Xrm.PowerShell.Tests.Fixtures;

namespace PentaWork.Xrm.PowerShell.Tests
{
    /// <summary>
    /// Covers the trigger-linking heuristics (self-recursion detection, cross-entity "this
    /// triggers"/"triggered by" links) and the enriched Markdown/Mermaid rendering in
    /// MainTemplate.sbn. This is genuinely new behavior (not a T4 port), so it's verified with
    /// targeted assertions rather than golden-file comparison.
    /// </summary>
    [TestClass]
    public class PluginGraphMarkdownTests
    {
        [TestMethod]
        public void LinkTriggers_DetectsSelfRecursion()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var accountGraph = entityGraphList.Single(g => g.EntityName == "account");
            var preOpStep = accountGraph.AllSteps.Single(s => s.Name == "Test.Plugins.AccountUpdatePreOpPlugin");

            Assert.IsTrue(preOpStep.HasSelfTrigger);
            Assert.AreEqual("update", preOpStep.SelfTrigger!.Message);
            Assert.AreEqual("account", preOpStep.SelfTrigger.EntityName);
            Assert.IsTrue(accountGraph.Summary.HasSelfRecursionWarnings);
            CollectionAssert.Contains(accountGraph.Summary.SelfRecursionWarnings, preOpStep);
        }

        [TestMethod]
        public void LinkTriggers_LinksCrossEntityTrigger_BothDirections()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var accountGraph = entityGraphList.Single(g => g.EntityName == "account");
            var taskGraph = entityGraphList.Single(g => g.EntityName == "task");

            var postOpStep = accountGraph.AllSteps.Single(s => s.Name == "Test.Plugins.AccountUpdatePostOpAsyncPlugin");
            var taskCreateStep = taskGraph.AllSteps.Single(s => s.Name == "Test.Plugins.TaskCreatePlugin");

            Assert.IsTrue(postOpStep.HasTriggeredOtherSteps);
            var link = postOpStep.TriggeredOtherSteps.Single();
            Assert.AreSame(taskCreateStep, link.OtherStep);
            Assert.AreEqual("create", link.Message);
            Assert.AreEqual("task", link.EntityName);

            Assert.IsTrue(taskCreateStep.HasTriggeredBy);
            var reverseLink = taskCreateStep.TriggeredBy.Single();
            Assert.AreSame(postOpStep, reverseLink.OtherStep);

            CollectionAssert.Contains(accountGraph.Summary.AffectedEntities, "task");
        }

        [TestMethod]
        public void ToMarkdown_AccountReport_ContainsSelfRecursionWarningAndLinkedCrossReference()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var accountMarkdown = entityGraphList.Single(g => g.EntityName == "account").ToMarkdown();

            StringAssert.Contains(accountMarkdown, "Possible self-recursion");
            StringAssert.Contains(accountMarkdown, "id=\"step-11111111-1111-1111-1111-111111111111\"");
            StringAssert.Contains(accountMarkdown, "[task](task.md)");
            StringAssert.Contains(accountMarkdown, "**Triggers**:");
            StringAssert.Contains(accountMarkdown, "[`Test.Plugins.TaskCreatePlugin`](task.md#step-33333333-3333-3333-3333-333333333333)");
        }

        [TestMethod]
        public void ToMarkdown_TaskReport_ContainsTriggeredByLinkBackToAccount()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var taskMarkdown = entityGraphList.Single(g => g.EntityName == "task").ToMarkdown();

            StringAssert.Contains(taskMarkdown, "**Triggered by**:");
            StringAssert.Contains(taskMarkdown, "[`Test.Plugins.AccountUpdatePostOpAsyncPlugin`](account.md#step-22222222-2222-2222-2222-222222222222)");
        }

        [TestMethod]
        public void ToMarkdown_AccountReport_RendersApiCallsSection()
        {
            // Regression test: Scriban's reflection binding doesn't resolve List<T>.Count (it
            // silently reads as null/empty), so a template check like "ApiCalls.Count > 0" always
            // evaluates false and the whole "API Calls" section silently disappears even when
            // ApiCalls is non-empty. Must use the precomputed PluginStepInfo.HasApiCalls instead.
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var accountGraph = entityGraphList.Single(g => g.EntityName == "account");
            var preOpStep = accountGraph.AllSteps.Single(s => s.Name == "Test.Plugins.AccountUpdatePreOpPlugin");
            Assert.IsTrue(preOpStep.HasApiCalls, "fixture step must actually have API calls for this test to be meaningful");

            var accountMarkdown = accountGraph.ToMarkdown();

            StringAssert.Contains(accountMarkdown, "**API Calls**:");
            StringAssert.Contains(accountMarkdown, "update - account");
            StringAssert.Contains(accountMarkdown, "**Used Attributes**: name");
            StringAssert.Contains(accountMarkdown, "Entity is Target");
        }

        [TestMethod]
        public void Summary_ReportsMessageBreakdownApiCallTotalAndCleanBillOfHealth()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var accountGraph = entityGraphList.Single(g => g.EntityName == "account");
            var taskGraph = entityGraphList.Single(g => g.EntityName == "task");

            Assert.AreEqual(2, accountGraph.Summary.TotalApiCalls);
            CollectionAssert.AreEquivalent(new[] { "Update" }, accountGraph.Summary.MessageCounts.Select(m => m.Message).ToList());
            Assert.AreEqual(2, accountGraph.Summary.MessageCounts.Single().Count);

            // "task" has neither self-recursion nor incoming/outgoing trigger noise worth flagging
            // as a warning - the summary should say so explicitly instead of just omitting the section.
            Assert.IsFalse(taskGraph.Summary.HasSelfRecursionWarnings);
            StringAssert.Contains(taskGraph.ToMarkdown(), "No self-recursion detected");
        }

        [TestMethod]
        public void LinkTriggers_ExcludesCandidateWhenNoFilteringAttributeIsTouched()
        {
            // "receiver" only fires on Update if "phone" or "email" changes. A caller that only
            // ever touches "name" should NOT be linked as triggering it.
            var caller = new PluginStepInfo
            {
                Id = Guid.NewGuid(),
                Name = "Caller",
                SdkMessage = "Update",
                PrimaryEntityName = "contact",
                Stage = Stage.PostOperation,
                Plugin = new PluginInfo { TypeName = "Caller" }
            };
            var receiver = new PluginStepInfo
            {
                Id = Guid.NewGuid(),
                Name = "Receiver",
                SdkMessage = "Update",
                PrimaryEntityName = "contact",
                Stage = Stage.PostOperation,
                FilteringAttributes = new List<string> { "phone", "email" },
                Plugin = new PluginInfo { TypeName = "Receiver" }
            };
            var apiCalls = new Dictionary<string, List<XrmApiCall>>
            {
                ["Caller"] = new List<XrmApiCall> { new XrmApiCall { Message = "update", EntityInfo = new EntityObj { LogicalName = "contact", UsedFields = new List<string> { "name" } } } },
                ["Receiver"] = new List<XrmApiCall>()
            };

            var entityGraphList = new EntityGraphList(apiCalls);
            entityGraphList.Add(caller);
            entityGraphList.Add(receiver);
            entityGraphList.LinkTriggers();

            Assert.IsFalse(caller.HasTriggeredOtherSteps, "caller only touches 'name', which isn't in receiver's FilteringAttributes");
            Assert.IsFalse(receiver.HasTriggeredBy);
        }

        [TestMethod]
        public void LinkTriggers_IncludesCandidateWhenAFilteringAttributeIsTouched()
        {
            var caller = new PluginStepInfo
            {
                Id = Guid.NewGuid(),
                Name = "Caller",
                SdkMessage = "Update",
                PrimaryEntityName = "contact",
                Stage = Stage.PostOperation,
                Plugin = new PluginInfo { TypeName = "Caller" }
            };
            var receiver = new PluginStepInfo
            {
                Id = Guid.NewGuid(),
                Name = "Receiver",
                SdkMessage = "Update",
                PrimaryEntityName = "contact",
                Stage = Stage.PostOperation,
                FilteringAttributes = new List<string> { "phone", "email" },
                Plugin = new PluginInfo { TypeName = "Receiver" }
            };
            var apiCalls = new Dictionary<string, List<XrmApiCall>>
            {
                ["Caller"] = new List<XrmApiCall> { new XrmApiCall { Message = "update", EntityInfo = new EntityObj { LogicalName = "contact", UsedFields = new List<string> { "name", "email" } } } },
                ["Receiver"] = new List<XrmApiCall>()
            };

            var entityGraphList = new EntityGraphList(apiCalls);
            entityGraphList.Add(caller);
            entityGraphList.Add(receiver);
            entityGraphList.LinkTriggers();

            Assert.IsTrue(caller.HasTriggeredOtherSteps, "caller touches 'email', which is one of receiver's FilteringAttributes");
            Assert.AreSame(receiver, caller.TriggeredOtherSteps.Single().OtherStep);
        }

        [TestMethod]
        public void ToMarkdown_HotEntityWithThousandsOfTriggerLinks_RendersWithoutExceedingScribanLoopLimit()
        {
            // Regression test: a real system had a "hot" entity/message combination shared by
            // more than Scriban's default 1000-iteration loop guard, causing
            // "Exceeding number of iteration limit `1000` for loop statement" at render time.
            // Reproduce that shape here (every one of ~1100 steps triggers every other one) and
            // confirm rendering succeeds and the output stays readable via the overflow cap.
            const int stepCount = 1100;
            var apiCalls = new Dictionary<string, List<XrmApiCall>>();
            var steps = new List<PluginStepInfo>();
            for (var i = 0; i < stepCount; i++)
            {
                var typeName = $"Test.Plugins.HotEntityPlugin{i}";
                steps.Add(new PluginStepInfo
                {
                    Id = Guid.NewGuid(),
                    Name = typeName,
                    SdkMessage = "Update",
                    PrimaryEntityName = "hotentity",
                    Stage = Stage.PostOperation,
                    Async = false,
                    Rank = 1,
                    Plugin = new PluginInfo { TypeName = typeName }
                });
                apiCalls[typeName] = new List<XrmApiCall> { new XrmApiCall { Message = "update", EntityInfo = new EntityObj { LogicalName = "hotentity" } } };
            }

            var entityGraphList = new EntityGraphList(apiCalls);
            steps.ForEach(entityGraphList.Add);
            entityGraphList.LinkTriggers();

            var hotEntityGraph = entityGraphList.Single(g => g.EntityName == "hotentity");
            var firstStep = hotEntityGraph.AllSteps.First();
            Assert.AreEqual(stepCount - 1, firstStep.TriggeredOtherSteps.Count, "every step should trigger every other step in this fixture");

            var markdown = hotEntityGraph.ToMarkdown();

            StringAssert.Contains(markdown, "...and");
            StringAssert.Contains(markdown, "more_");
        }

        [TestMethod]
        public void ToMarkdown_Diagram_StylesAsyncAndSelfRecursionAndDrawsTriggerEdge()
        {
            var entityGraphList = PluginGraphFixture.BuildEntityGraphList();
            var accountMarkdown = entityGraphList.Single(g => g.EntityName == "account").ToMarkdown();

            StringAssert.Contains(accountMarkdown, "```mermaid");
            StringAssert.Contains(accountMarkdown, "classDef selfRecursion");
            StringAssert.Contains(accountMarkdown, "classDef asyncStep");
            StringAssert.Contains(accountMarkdown, "class n22222222222222222222222222222222 asyncStep");
            StringAssert.Contains(accountMarkdown, "class n11111111111111111111111111111111 selfRecursion");
            StringAssert.Contains(accountMarkdown, "-.->|\"possible recursion\"|");
        }
    }
}
