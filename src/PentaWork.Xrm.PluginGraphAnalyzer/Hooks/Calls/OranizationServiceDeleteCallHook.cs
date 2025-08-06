using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceDeleteCallHook : IHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "delete";
            apiCall.EntityInfo = new EntityObj();
            apiCall.EntityInfo.LogicalName = (string)parameters[1];

            stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {stack.Count}] Return value from {method.FullName}");

            return apiCall;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.IOrganizationService::Delete(System.String,System.Guid)";
    }
}
