using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly;
using Account = PentaWork.Xrm.Tests.Plugins.Proxy.Account;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginsServiceWithGenerics : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Name = "Test";

            var testGeneric = new TestGeneric<Account>();
            testGeneric.Execute(service, entity);
        }
    }
}
