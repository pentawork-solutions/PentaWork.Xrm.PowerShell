using Microsoft.Xrm.Sdk;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly;
using PentaWork.Xrm.Tests.PluginsTestSideAssembly.Proxy;
using System.Diagnostics;

namespace PentaWork.Xrm.Tests.Plugins
{
    public class TestPluginWithContextWrapper : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);
            var wrapper = new TestServiceContextWrapper(service);

            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            wrapper.Context.AddObject(entity);
            wrapper.Context.SaveChanges();
        }
    }

    public class TestPluginWithReturnInExecute : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            // Add a return statement in the execute method to test branching
            if (entity.Address1_Line1.Contains("TEST")) return;

            service.Create(entity);
        }
    }

    public class TestPluginWithTryCatchInExecute : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            try
            {
                service.Create(entity);
            }
            catch (CustomException ex) { Debug.WriteLine(ex); }
            finally { Debug.WriteLine("Finally"); }
        }
    }

    public class TestPluginWithThrow : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Name = "Test";
            entity.Address1_Line1 = "Test Street 1";

            try
            {
                service.Create(entity);
            }
            catch (CustomException) { throw; }
            catch (Exception ex) { Debug.WriteLine(ex); }
            finally { Debug.WriteLine("Finally"); }
        }
    }

    public class TestPluginWithRecursions : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Address1_Line1 = "Test Street 1";

            entity = Test(entity);
            service.Create(entity);
        }

        public Account Test(Account entity)
        {
            if (entity.Name == "Test") return entity;
            else if (entity.Name == string.Empty)
            {
                entity.Name = "None";
                return Test(entity);
            }
            return Test(entity);
        }
    }

    public class TestPluginWithRecursions2 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Address1_Line1 = "Test Street 1";

            entity = Test(entity);
            service.Create(entity);
        }

        public Account Test(Account entity)
        {
            if (entity.Name == "Test") return Test(entity);
            else if (entity.Name == string.Empty)
            {
                entity.Name = "None";
                return Test(entity);
            }
            else
            {
                entity.Address1_City = "Test";
            }
            return entity;
        }
    }

    public class TestPluginWithLoop : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Address1_Line1 = "Test Street 1";

            entity = Test(entity);
            service.Create(entity);
        }

        public Account Test(Account entity)
        {
            for (var i = 0; i < 10; i++)
            {
                entity.Name = "Test";
            }
            return entity;
        }
    }

    public class TestPluginWithSwitchCase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Address1_Line1 = "Test Street 1";

            entity = Test(entity);
            service.Create(entity);
        }

        public Account Test(Account entity)
        {
            // Forces IL Switch Code
            int number = 2;
            switch (number)
            {
                case 1:
                    entity.Address1_City = "Test";
                    break;
                case 2:
                    entity.Address1_Country = "Test";
                    break;
                case 3:
                    entity.Address1_County = "Test";
                    break;
                default:
                    entity.Address1_Fax = "Test";
                    break;
            }
            return entity;
        }
    }

    public class CustomException : Exception { }
}
