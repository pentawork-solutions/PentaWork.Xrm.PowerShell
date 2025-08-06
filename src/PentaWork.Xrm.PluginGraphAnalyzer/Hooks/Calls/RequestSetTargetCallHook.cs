using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class RequestSetTargetCallHook : IHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            var apiCall = (XrmApiCall)parameters[0];
            apiCall.EntityInfo = (EntityObj)parameters[1];

            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            parameters.Count > 1
            && parameters[1] is EntityObj
            && method.FullName is
                "System.Void Microsoft.Xrm.Sdk.Messages.CreateRequest::set_Target(Microsoft.Xrm.Sdk.Entity)" or
                "System.Void Microsoft.Xrm.Sdk.Messages.UpdateRequest::set_Target(Microsoft.Xrm.Sdk.Entity)" or
                "System.Void Microsoft.Xrm.Sdk.Messages.DeleteRequest::set_Target(Microsoft.Xrm.Sdk.EntityReference)";
    }
}
