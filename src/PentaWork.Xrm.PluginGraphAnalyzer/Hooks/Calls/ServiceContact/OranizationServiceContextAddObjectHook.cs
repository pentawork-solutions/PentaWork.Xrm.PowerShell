using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.ServiceContact
{
    internal class OranizationServiceContextAddObjectHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "create";
            apiCall.EntityInfo = (EntityObj)parameters[1];
            apiCall.IsExecuted = false;

            var serviceContext = (ServiceContextObj)parameters[0];
            serviceContext.AddCall(apiCall);

            storageFrame.ApiCalls.Add(apiCall);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 1
            && parameters[1] is EntityObj
            && method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::AddObject(Microsoft.Xrm.Sdk.Entity)";
    }
}
