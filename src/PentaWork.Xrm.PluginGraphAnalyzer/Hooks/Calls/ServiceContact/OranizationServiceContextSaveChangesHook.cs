using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.ServiceContact
{
    internal class OranizationServiceContextSaveChangesHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var serviceContext = (ServiceContextObj)parameters[0];
            serviceContext.MarkCallsExecuted();

            storageFrame.Stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {storageFrame.Stack.Count}] Return value from {method.FullName}");
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            method.FullName == "Microsoft.Xrm.Sdk.SaveChangesResultCollection Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::SaveChanges()";
    }
}
