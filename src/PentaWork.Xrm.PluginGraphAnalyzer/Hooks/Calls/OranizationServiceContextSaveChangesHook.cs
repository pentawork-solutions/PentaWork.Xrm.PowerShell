using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceContextSaveChangesHook : ICallHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var serviceContext = (ServiceContextObj)parameters[0];
            serviceContext.MarkCallsExecuted();

            vmData.Stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {vmData.Stack.Count}] Return value from {method.FullName}");
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "Microsoft.Xrm.Sdk.SaveChangesResultCollection Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::SaveChanges()";
    }
}
