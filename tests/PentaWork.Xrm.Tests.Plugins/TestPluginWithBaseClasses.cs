using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly.Proxy;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginWithBaseClasses : BasePlugin, IPlugin
    {
        public override void ExecutePlugin(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }

    public abstract class BasePlugin : IPlugin
    {
        public abstract void ExecutePlugin(IServiceProvider serviceProvider);

        public void Execute(IServiceProvider serviceProvider)
        {
            ExecutePlugin(serviceProvider);
        }
    }
}
