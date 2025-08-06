using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OrganizationRequestCtor : IHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            var genObj = (GenericObj)parameters[0];
            if (method.FullName.Contains("CreateRequest"))
                genObj.Fields["OrganizationRequest.MessageName"] = "create";
            else if (method.FullName.Contains("UpdateRequest"))
                genObj.Fields["OrganizationRequest.MessageName"] = "update";
            else if (method.FullName.Contains("DeleteRequest"))
                genObj.Fields["OrganizationRequest.MessageName"] = "delete";
            else
                genObj.Fields["OrganizationRequest.MessageName"] = (string)parameters[1];
            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName is
            "System.Void Microsoft.Xrm.Sdk.Messages.CreateRequest::.ctor()" or
            "System.Void Microsoft.Xrm.Sdk.Messages.UpdateRequest::.ctor()" or
            "System.Void Microsoft.Xrm.Sdk.Messages.DeleteRequest::.ctor()" or
            "System.Void Microsoft.Xrm.Sdk.OrganizationRequest::.ctor(System.String)";
    }
}
