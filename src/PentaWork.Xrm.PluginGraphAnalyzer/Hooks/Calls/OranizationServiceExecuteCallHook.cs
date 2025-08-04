using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceExecuteCallHook : ICallHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            var apiCall = (XrmApiCall)parameters[1];
            apiCall.IsExecuted = true;

            stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {stack.Count}] Return value from {method.FullName}");

            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            parameters.Count > 1
            && parameters[1] is XrmApiCall
            && method.FullName == "Microsoft.Xrm.Sdk.OrganizationResponse Microsoft.Xrm.Sdk.IOrganizationService::Execute(Microsoft.Xrm.Sdk.OrganizationRequest)";
    }
}
