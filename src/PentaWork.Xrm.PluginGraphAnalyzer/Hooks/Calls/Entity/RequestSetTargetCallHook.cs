using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.Entity
{
    internal class RequestSetTargetCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var apiCall = (XrmApiCall)parameters[0];
            apiCall.EntityInfo = (EntityObj)parameters[1];
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 1
            && parameters[1] is EntityObj
            && method.FullName is
                "System.Void Microsoft.Xrm.Sdk.Messages.CreateRequest::set_Target(Microsoft.Xrm.Sdk.Entity)" or
                "System.Void Microsoft.Xrm.Sdk.Messages.UpdateRequest::set_Target(Microsoft.Xrm.Sdk.Entity)" or
                "System.Void Microsoft.Xrm.Sdk.Messages.DeleteRequest::set_Target(Microsoft.Xrm.Sdk.EntityReference)";
    }
}
