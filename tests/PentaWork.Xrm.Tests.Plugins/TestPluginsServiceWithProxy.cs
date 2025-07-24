using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly;
using Account = PentaWork.Xrm.Tests.Plugins.Proxy.Account;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginServiceWithProxyCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
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

    public class TestPluginServiceWithProxyUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Id = Guid.NewGuid();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Update(entity);
        }
    }

    public class TestPluginServiceInMethodWithProxyCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            TestMethod(service);
        }

        private void TestMethod(IOrganizationService service)
        {
            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }

    public class TestPluginServiceInSideMethodWithProxyCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var sideClass = new TestSideClass();
            sideClass.TestMethod(service);
        }
    }

    public class TestPluginServiceDirectWithSideProxyCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new PluginsTestSideAssembly.Proxy.Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }
}
