using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.IOrganizationContext
{
    internal class OranizationServiceDeleteCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "delete";
            apiCall.EntityInfo = new EntityObj();
            apiCall.EntityInfo.LogicalName = (string)parameters[1];

            storageFrame.Stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {storageFrame.Stack.Count}] Return value from {method.FullName}");

            storageFrame.ApiCalls.Add(apiCall);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.IOrganizationService::Delete(System.String,System.Guid)";
    }
}
