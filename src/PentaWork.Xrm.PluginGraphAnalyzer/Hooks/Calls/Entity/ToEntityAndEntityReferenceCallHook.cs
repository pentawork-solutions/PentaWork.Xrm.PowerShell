using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.Entity
{
    internal class ToEntityAndEntityReferenceCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            storageFrame.Stack.Push(parameters[0]); // Push EntityObj back onto the stack
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 0
            && parameters[0] is EntityObj
            && (method.FullName == "Microsoft.Xrm.Sdk.EntityReference Microsoft.Xrm.Sdk.Entity::ToEntityReference()"
                || method.FullName == "T Microsoft.Xrm.Sdk.Entity::ToEntity<T>()");
    }
}
