using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class OrganizationRequestCtor : IHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            var genObj = (GenericObj)parameters[0];
            genObj.Fields["OrganizationRequest.MessageName"] = "create";
            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.Messages.CreateRequest::.ctor()";
    }
}
