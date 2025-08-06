using Microsoft.Xrm.Sdk;

namespace PentaWork.Xrm.Tests.PluginsTestSideAssembly
{
    public class TestGeneric<T> where T : Entity
    {
        public void Execute(IOrganizationService service, T entity)
        {
            entity["address1_line1"] = "Test Street 1";
            service.Create(entity);
        }
    }
}
