using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraphTests
{
    [TestClass]
    public sealed class XrmApiCallTests
    {
        [TestMethod]
        public void DisplayLine_CustomApiCallWithoutTargetEntity_HasNoDanglingDash()
        {
            var apiCall = new XrmApiCall { Message = "new_MyCustomApi", EntityInfo = null };

            Assert.AreEqual("- new_MyCustomApi", apiCall.DisplayLine);
        }

        [TestMethod]
        public void DisplayLine_EntityCallWithTargetEntity_IncludesEntityName()
        {
            var apiCall = new XrmApiCall { Message = "create", EntityInfo = new EntityObj { LogicalName = "account" } };

            Assert.AreEqual("- create - account", apiCall.DisplayLine);
        }

        [TestMethod]
        public void DisplayLine_NotExecutedCall_AppendsMarker()
        {
            var apiCall = new XrmApiCall { Message = "create", EntityInfo = new EntityObj { LogicalName = "account" }, IsExecuted = false };

            Assert.AreEqual("- create - account *(not executed)*", apiCall.DisplayLine);
        }
    }
}
