using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.IOrganizationContext
{
    internal class OranizationServiceCreateCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "create";
            apiCall.EntityInfo = (EntityObj)parameters[1];

            storageFrame.Stack.Push($"Dummy return value for '{method.FullName}'");
            Debug.WriteLine($"[↑ {storageFrame.Stack.Count}] Return value from {method.FullName}");

            storageFrame.ApiCalls.Add(apiCall);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 1
            && parameters[1] is EntityObj
            && method.FullName == "System.Guid Microsoft.Xrm.Sdk.IOrganizationService::Create(Microsoft.Xrm.Sdk.Entity)";
    }
}
