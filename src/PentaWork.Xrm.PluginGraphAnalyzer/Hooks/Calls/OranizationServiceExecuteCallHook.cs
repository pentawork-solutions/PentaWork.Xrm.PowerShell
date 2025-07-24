using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceExecuteCallHook : ICallHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var apiCall = (XrmApiCall)parameters[1];
            apiCall.IsExecuted = true;

            vmData.Stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {vmData.Stack.Count}] Return value from {method.FullName}");
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
             method.FullName == "Microsoft.Xrm.Sdk.OrganizationResponse Microsoft.Xrm.Sdk.IOrganizationService::Execute(Microsoft.Xrm.Sdk.OrganizationRequest)" && parameters[1] is XrmApiCall;
    }
}
