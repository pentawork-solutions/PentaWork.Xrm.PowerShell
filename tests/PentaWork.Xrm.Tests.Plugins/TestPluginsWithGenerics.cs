using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly.Proxy;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginsWithGenerics : IPlugin
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

    public class TestPluginWithActivator : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = Activator.CreateInstance<Account>();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }

    public class TestPluginWithGenericActivator : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entityCreator = new TestGenericActivator<Account>();
            var entity = entityCreator.Execute();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }

    public class TestPluginWithGenericActivatorMethod : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entityCreator = new TestGenericActivatorMethod();
            var entity = entityCreator.Execute<Account>();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }

    public class TestPluginWithGenericBase : BasePlugin<Account>, IPlugin
    {
        public new void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);
        }
    }

    public abstract class BasePlugin<T> : IPlugin where T : Entity
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = Activator.CreateInstance<T>();
            entity["name"] = "Test";
            entity["street1"] = "Test Street 1";

            service.Create(entity);
        }
    }
}
