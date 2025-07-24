using Microsoft.Xrm.Sdk;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginServiceCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Entity("account");
            entity.Attributes["name"] = "Test";
            entity.Attributes["street1"] = "Test Street 1";

            service.Create(entity);
        }
    }

    public class TestPluginServiceUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Entity("account");
            entity.Id = Guid.NewGuid();
            entity.Attributes["name"] = "Test";
            entity.Attributes["street1"] = "Test Street 1";

            service.Update(entity);
        }
    }

    public class TestPluginServiceDelete : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            service.Delete("account", Guid.NewGuid());
        }
    }
}
