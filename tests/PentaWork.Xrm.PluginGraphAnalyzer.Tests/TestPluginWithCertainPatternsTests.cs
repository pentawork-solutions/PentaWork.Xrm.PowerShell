using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraphTests
{
    [TestClass]
    public sealed class TestPluginWithCertainPatternsTests
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
            var ctx = ModuleDef.CreateModuleContext();
            var asmResolver = (AssemblyResolver)ctx.AssemblyResolver;
            asmResolver.EnableTypeDefCache = true;

            foreach (var assemblyFile in assemblyList)
            {
                var module = ModuleDefMD.Load(assemblyFile, ctx);
                module.Context = ctx;
                asmResolver.AddToCache(module);

                moduleList.Add(module);
            }
            _moduleList = new Dictionary<Guid, PluginModuleList>
            {
                { new Guid("f305f42a-c37a-487a-a4b0-521b946315b6"), moduleList }
            };
        }

        [TestMethod]
        public void ShouldAnalyseServiceCreateWithReturnInExcuteSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithReturnInExecute";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseServiceCreateWithTryCatchInExcuteSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithTryCatchInExecute";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseServiceCreateWithThrowSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithThrow";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseWithServiceContextWrapperSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithContextWrapper";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseRecursionSuccessfullyWithoutInfinityLoop()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithRecursions";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseRecursionSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithRecursions";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.IsTrue(pluginApiCalls.FirstOrDefault()?.EntityInfo.CallLoopHit);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseLoopSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithLoop";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseSwitchCaseSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithSwitchCase";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(5, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseArithmeticOperatorsSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithArithmeticOperators";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("update", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldIsolateExceptionsPerPluginAndStillAnalyseTheOthers()
        {
            // Arrange
            var brokenStep = new PluginStepInfo
            {
                Plugin = new PluginInfo
                {
                    AssemblyInfo = new AssemblyInfo { Name = "PentaWork.Xrm.Tests.Plugins" },
                    PackageInfo = new PackageInfo { Id = new Guid("f305f42a-c37a-487a-a4b0-521b946315b6") },
                    TypeName = "PentaWork.Xrm.Tests.Plugins.ThisTypeDoesNotExist"
                }
            };
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithReturnInExecute";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [brokenStep, _pluginStepInfo], "PentaWork.Xrm.Tests.*");

            // Assert
            Assert.IsTrue(apiCalls.ContainsKey("PentaWork.Xrm.Tests.Plugins.ThisTypeDoesNotExist"));
            Assert.AreEqual(0, apiCalls["PentaWork.Xrm.Tests.Plugins.ThisTypeDoesNotExist"].Count);

            var validApiCalls = apiCalls["PentaWork.Xrm.Tests.Plugins.TestPluginWithReturnInExecute"];
            Assert.AreEqual("create", validApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual("account", validApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldFallBackToACleanPlaceholderWhenTheCustomApiNameCannotBeResolved()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithComputedApiConstant";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("<TestConstants.ComputedApiName>", pluginApiCalls.FirstOrDefault()?.Message);
        }

        [TestMethod]
        public void ShouldAnalyseSwitchCaseWithoutDuplicatingApiCalls()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithSwitchCaseMultipleApiCalls";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            // HandleBranch treats "everything textually between the switch and a target" as that
            // target's body (a separate, pre-existing quirk out of scope here), so a 3-case switch
            // doesn't yield exactly one call per case - but that per-call count must not additionally
            // be multiplied by the number of cases anymore. Before this fix, the outer duplication
            // bug made this exact plugin produce 10 calls instead of these 4 (verified by reverting
            // the fix locally); it must now be seen exactly once.
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual(4, pluginApiCalls.Count);
            Assert.AreEqual(2, pluginApiCalls.Count(c => c.Message == "create"));
            Assert.AreEqual(1, pluginApiCalls.Count(c => c.Message == "update"));
            Assert.AreEqual(1, pluginApiCalls.Count(c => c.Message == "delete"));
        }

        [TestMethod]
        public void ShouldAnalyseBoundedSelfRecursionAndStillDetectTheFollowingCreateCall()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithBoundedSelfRecursion";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
            Assert.AreEqual(2, pluginApiCalls.FirstOrDefault()?.EntityInfo.UsedFields.Count);
        }

        [TestMethod]
        public void ShouldTrackLogicalNameReassignmentSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithLogicalNameReassignment";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("create", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual("contact", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseAssociateSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithAssociate";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("associate", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }

        [TestMethod]
        public void ShouldAnalyseDisassociateSuccessfully()
        {
            // Arrange
            _pluginStepInfo.Plugin.TypeName = "PentaWork.Xrm.Tests.Plugins.TestPluginWithDisassociate";

            // Act
            var apiCalls = _pluginGraphAnalyzer.AnalyzeApiCalls(_moduleList, [_pluginStepInfo], "PentaWork.Xrm.Tests.*");
            var pluginApiCalls = apiCalls.FirstOrDefault().Value;

            // Assert
            Assert.IsNotNull(pluginApiCalls);
            Assert.AreEqual("disassociate", pluginApiCalls.FirstOrDefault()?.Message);
            Assert.AreEqual("account", pluginApiCalls.FirstOrDefault()?.EntityInfo.LogicalName);
        }
    }
}
