using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.Entity
{
    internal class EntityGetLogicalNameCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var entity = (EntityObj)parameters[0];
            storageFrame.Stack.Push(entity.LogicalName);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 0
            && parameters[0] is EntityObj
            && method.FullName == "System.String Microsoft.Xrm.Sdk.Entity::get_LogicalName()";
    }
}
