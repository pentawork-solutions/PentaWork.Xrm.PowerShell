using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.Entity
{
    internal class SetAttributeCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var entity = (EntityObj)parameters[0];
            if (!entity.UsedFields.Contains((string)parameters[1]))
                entity.UsedFields.Add((string)parameters[1]);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 1 && parameters[0] is EntityObj && parameters[1] is string && method.FullName is
                "System.Void Microsoft.Xrm.Sdk.Entity::SetAttributeValue(System.String,System.Object)" or
                "System.Void Microsoft.Xrm.Sdk.DataCollection`2<System.String,System.Object>::set_Item(System.String,System.Object)" or
                "System.Void Microsoft.Xrm.Sdk.Entity::set_Item(System.String,System.Object)";
    }
}
