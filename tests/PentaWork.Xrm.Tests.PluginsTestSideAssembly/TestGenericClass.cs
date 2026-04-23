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

    public class TestGenericActivator<T> where T : Entity
    {
        public T Execute()
        {
            return Activator.CreateInstance<T>();
        }
    }

    public class TestGenericActivatorMethod
    {
        public T Execute<T>() where T : Entity
        {
            return Activator.CreateInstance<T>();
        }
    }
}
