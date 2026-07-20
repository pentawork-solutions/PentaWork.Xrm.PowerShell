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

    public class TestPluginWithArithmeticOperators : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Address1_Line1 = "Test Street 1";

            // Forces IL Mul/Rem/Shl opcodes
            int a = 6;
            int b = 4;
            int calculated = (a * b) % 5 << 1;
            entity.NumberOfEmployees = calculated;

            service.Update(entity);
        }
    }

    public class TestPluginWithBoundedSelfRecursion : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Address1_Line1 = "Test Street 1";

            entity = BuildHierarchy(entity, 0);
            service.Create(entity);
        }

        // Recurses exactly once (harmless, bounded self-recursion) before returning - the
        // subsequent Create call must still see a real entity, not a generic loop-detection dummy.
        public Account BuildHierarchy(Account entity, int depth)
        {
            if (depth >= 1) return entity;
            entity.Name = "Child";
            return BuildHierarchy(entity, depth + 1);
        }
    }

    public class TestPluginWithSwitchCaseMultipleApiCalls : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.Address1_Line1 = "Test Street 1";

            // Forces IL Switch Code - each case makes its own distinct API call so a
            // double-interpretation bug in the Switch handling would show up as extra XrmApiCalls.
            int number = 2;
            switch (number)
            {
                case 1:
                    service.Create(entity);
                    break;
                case 2:
                    service.Update(entity);
                    break;
                case 3:
                    service.Delete("account", entity.Id);
                    break;
            }
        }
    }

    public class TestPluginWithLogicalNameReassignment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var entity = new Account();
            entity.LogicalName = "contact"; // Forces Entity.set_LogicalName IL call
            entity.Address1_Line1 = "Test Street 1";

            service.Create(entity);
        }
    }

    public class TestPluginWithAssociate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var relationship = new Relationship("new_account_contact");
            var relatedEntities = new EntityReferenceCollection
            {
                new EntityReference("contact", Guid.NewGuid())
            };

            service.Associate("account", Guid.NewGuid(), relationship, relatedEntities);
        }
    }

    public class TestPluginWithDisassociate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var relationship = new Relationship("new_account_contact");
            var relatedEntities = new EntityReferenceCollection
            {
                new EntityReference("contact", Guid.NewGuid())
            };

            service.Disassociate("account", Guid.NewGuid(), relationship, relatedEntities);
        }
    }

    public static class TestConstants
    {
        public static readonly string MyCustomApiName = "new_MyCustomApi";
        // Aliases another static readonly field instead of a literal - resolving this needs to
        // follow the Ldsfld chain, not just look for an adjacent Ldstr/Stsfld pair.
        public static readonly string AliasedApiName = MyCustomApiName;
        // Assembled via a method call the resolver intentionally doesn't try to follow - this must
        // fall back to a clean placeholder instead of a raw IL instruction dump.
        public static readonly string ComputedApiName = string.Concat("new_", "ComputedApi");
    }

    public class TestPluginWithCustomApiConstant : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var request = new OrganizationRequest(TestConstants.MyCustomApiName);
            service.Execute(request);
        }
    }

    public class TestPluginWithAliasedApiConstant : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var request = new OrganizationRequest(TestConstants.AliasedApiName);
            service.Execute(request);
        }
    }

    public class TestPluginWithComputedApiConstant : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.UserId);

            var request = new OrganizationRequest(TestConstants.ComputedApiName);
            service.Execute(request);
        }
    }

    public class CustomException : Exception { }
}
