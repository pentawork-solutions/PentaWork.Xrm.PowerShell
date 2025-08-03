using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceCreateCallHook : ICallHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, ref Stack<object> stack)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "create";
            apiCall.EntityInfo = (EntityObj)parameters[1];

            stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {stack.Count}] Return value from {method.FullName}");

            return apiCall;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Guid Microsoft.Xrm.Sdk.IOrganizationService::Create(Microsoft.Xrm.Sdk.Entity)";
    }
}
