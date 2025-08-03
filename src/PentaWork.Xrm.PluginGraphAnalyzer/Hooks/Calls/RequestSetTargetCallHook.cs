using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class RequestSetTargetCallHook : ICallHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, ref Stack<object> stack)
        {
            var apiCall = (XrmApiCall)parameters[0];
            apiCall.EntityInfo = (EntityObj)parameters[1];

            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.Messages.CreateRequest::set_Target(Microsoft.Xrm.Sdk.Entity)" && parameters[1] is EntityObj;
    }
}
