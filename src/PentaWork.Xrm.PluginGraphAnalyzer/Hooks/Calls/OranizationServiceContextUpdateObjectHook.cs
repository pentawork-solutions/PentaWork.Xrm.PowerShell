using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceContextUpdateObjectHook : ICallHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "update";
            apiCall.EntityInfo = (EntityObj)parameters[1];
            apiCall.IsExecuted = false;

            vmData.ApiCalls.Add(apiCall);

            var serviceContext = (ServiceContextObj)parameters[0];
            serviceContext.AddCall(apiCall);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::UpdateObject(Microsoft.Xrm.Sdk.Entity)";
    }
}
