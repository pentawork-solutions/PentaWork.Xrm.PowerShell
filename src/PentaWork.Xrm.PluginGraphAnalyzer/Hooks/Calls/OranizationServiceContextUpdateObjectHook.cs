using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OranizationServiceContextUpdateObjectHook : ICallHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "update";
            apiCall.EntityInfo = (EntityObj)parameters[1];
            apiCall.IsExecuted = false;

            var serviceContext = (ServiceContextObj)parameters[0];
            serviceContext.AddCall(apiCall);

            return apiCall;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            parameters.Count > 1
            && parameters[1] is EntityObj
            && method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::UpdateObject(Microsoft.Xrm.Sdk.Entity)";
    }
}
