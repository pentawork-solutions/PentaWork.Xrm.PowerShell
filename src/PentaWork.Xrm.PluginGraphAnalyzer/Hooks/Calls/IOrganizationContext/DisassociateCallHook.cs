using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.IOrganizationContext
{
    internal class DisassociateCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "disassociate";
            apiCall.EntityInfo = new EntityObj { LogicalName = (string)parameters[1] };

            storageFrame.ApiCalls.Add(apiCall);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.IOrganizationService::Disassociate(System.String,System.Guid,Microsoft.Xrm.Sdk.Relationship,Microsoft.Xrm.Sdk.EntityReferenceCollection)";
    }
}
