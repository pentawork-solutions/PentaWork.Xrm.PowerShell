using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.IOrganizationContext
{
    internal class OranizationServiceExecuteCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var apiCall = (XrmApiCall)parameters[1];
            apiCall.IsExecuted = true;

            storageFrame.Stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {storageFrame.Stack.Count}] Return value from {method.FullName}");
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 1
            && parameters[1] is XrmApiCall
            && method.FullName == "Microsoft.Xrm.Sdk.OrganizationResponse Microsoft.Xrm.Sdk.IOrganizationService::Execute(Microsoft.Xrm.Sdk.OrganizationRequest)";
    }
}
