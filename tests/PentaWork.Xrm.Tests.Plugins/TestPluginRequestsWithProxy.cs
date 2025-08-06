using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Account = PentaWork.Xrm.Tests.Plugins.Proxy.Account;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginRequestsWithProxyCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            var createRequest = new CreateRequest
            {
                Target = entity
            };

            service.Execute(createRequest);
        }
    }

    public class TestPluginRequestsWithProxyUpdate : IPlugin
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

            var updateRequest = new UpdateRequest
            {
                Target = entity
            };

            service.Execute(updateRequest);
        }
    }

    public class TestPluginRequestsWithProxyDelete : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Id = Guid.NewGuid();

            var deleteRequest = new DeleteRequest
            {
                Target = entity.ToEntityReference()
            };

            service.Execute(deleteRequest);
        }
    }

    public class TestPluginRequestsWithProxyExecute : IPlugin
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

            var request = new OrganizationRequest("pw_TestMessage")
            {
                ["account"] = entity.ToEntityReference()
            };

            service.Execute(request);
        }
    }
}
