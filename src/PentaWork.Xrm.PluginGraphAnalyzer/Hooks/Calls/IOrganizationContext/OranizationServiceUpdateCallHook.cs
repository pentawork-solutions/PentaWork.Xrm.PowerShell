using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.IOrganizationContext
{
    internal class OranizationServiceUpdateCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "update";
            apiCall.EntityInfo = (EntityObj)parameters[1];

            storageFrame.ApiCalls.Add(apiCall);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 1
            && parameters[1] is EntityObj
            && method.FullName == "System.Void Microsoft.Xrm.Sdk.IOrganizationService::Update(Microsoft.Xrm.Sdk.Entity)";
    }
}
