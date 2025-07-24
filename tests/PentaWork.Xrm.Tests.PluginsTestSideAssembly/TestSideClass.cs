using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly.Proxy;

namespace PentaWork.Xrm.Tests.PluginsTestSideAssembly
{
    public class TestSideClass
    {
        public void TestMethod(IOrganizationService service)
        {
            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }
}
