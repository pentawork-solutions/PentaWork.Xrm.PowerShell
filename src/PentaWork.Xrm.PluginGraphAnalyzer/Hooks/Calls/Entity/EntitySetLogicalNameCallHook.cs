using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.Entity
{
    internal class EntitySetLogicalNameCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var entity = (EntityObj)parameters[0];
            entity.LogicalName = (string)parameters[1];
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 1
            && parameters[0] is EntityObj
            && method.FullName == "System.Void Microsoft.Xrm.Sdk.Entity::set_LogicalName(System.String)";
    }
}
