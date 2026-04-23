using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly.Proxy;

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

    public class TestPluginServiceContextWithProxyDelete : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);
            var serviceContext = new OrganizationServiceContext(service);

            var entity = new Account();
            entity.Id = Guid.NewGuid();

            serviceContext.Attach(entity);
            serviceContext.DeleteObject(entity);
            serviceContext.SaveChanges();
        }
    }

    public class TestPluginServiceContextWithProxyDeleteNoSave : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);
            var serviceContext = new OrganizationServiceContext(service);

            var entity = new Account();
            entity.Id = Guid.NewGuid();

            serviceContext.Attach(entity);
            serviceContext.DeleteObject(entity);
            // Don't save to check, if the graph analyzer recognizes it
        }
    }

    public class TestPluginServiceContextWithProxyNoSaveButOtherContext : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);
            var serviceContext = new OrganizationServiceContext(service);
            var serviceContext2 = new OrganizationServiceContext(service);

            var entity = new Account();
            entity.Id = Guid.NewGuid();

            var entity2 = new Account();
            entity2.Id = Guid.NewGuid();

            serviceContext.Attach(entity);
            serviceContext.DeleteObject(entity);
            // Don't save to check, if the graph analyzer recognizes it

            serviceContext2.Attach(entity);
            serviceContext2.DeleteObject(entity);
            serviceContext2.SaveChanges();
        }
    }

    public class TestPluginServiceContextWithProxyClearsChangesAndAddsSomeAgain : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);
            var serviceContext = new OrganizationServiceContext(service);

            var entity = new Account();
            entity.Id = Guid.NewGuid();

            serviceContext.Attach(entity);
            serviceContext.DeleteObject(entity);
            serviceContext.ClearChanges();

            var entity2 = new Account();
            entity2.Id = Guid.NewGuid();

            serviceContext.Attach(entity2);
            serviceContext.DeleteObject(entity2);
            serviceContext.SaveChanges();
        }
    }
}
