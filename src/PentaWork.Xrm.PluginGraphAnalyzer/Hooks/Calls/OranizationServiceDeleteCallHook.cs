using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceDeleteCallHook : ICallHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "delete";
            apiCall.EntityInfo = new EntityObj();
            apiCall.EntityInfo.LogicalName = (string)parameters[1];

            vmData.ApiCalls.Add(apiCall);

            vmData.Stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {vmData.Stack.Count}] Return value from {method.FullName}");
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.IOrganizationService::Delete(System.String,System.Guid)";
    }
}
