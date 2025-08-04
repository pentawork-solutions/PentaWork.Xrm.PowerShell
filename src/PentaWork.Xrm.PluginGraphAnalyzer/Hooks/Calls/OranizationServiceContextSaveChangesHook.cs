using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceContextSaveChangesHook : ICallHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            var serviceContext = (ServiceContextObj)parameters[0];
            serviceContext.MarkCallsExecuted();

            stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {stack.Count}] Return value from {method.FullName}");

            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "Microsoft.Xrm.Sdk.SaveChangesResultCollection Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::SaveChanges()";
    }
}
