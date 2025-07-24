using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Account = PentaWork.Xrm.Tests.Plugins.Proxy.Account;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginServiceContextWithProxyCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);
            var serviceContext = new OrganizationServiceContext(service);

            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            serviceContext.AddObject(entity);
            serviceContext.SaveChanges();
        }
    }

    public class TestPluginServiceContextWithProxyUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);
            var serviceContext = new OrganizationServiceContext(service);

            var entity = new Account();
            entity.Id = Guid.NewGuid();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            serviceContext.Attach(entity);
            serviceContext.UpdateObject(entity);
            serviceContext.SaveChanges();
        }
    }
}
