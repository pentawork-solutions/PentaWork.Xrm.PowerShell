using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Creations
{
    internal class EntityCreationHook : ICreationHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var entity = new EntityObj();
            entity.LogicalName = (string)parameters[0];
            vmData.Stack.Push(entity);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.Entity::.ctor(System.String)";
    }
}
