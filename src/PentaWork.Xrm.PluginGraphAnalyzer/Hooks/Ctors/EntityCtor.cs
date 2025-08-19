using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class EntityCtor : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var genObj = (GenericObj)parameters[0];
            genObj.Fields["EntityObj.LogicalName"] = (string)parameters[1];
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.Entity::.ctor(System.String)";
    }
}
