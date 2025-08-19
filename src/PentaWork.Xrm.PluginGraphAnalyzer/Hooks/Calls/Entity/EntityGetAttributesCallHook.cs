using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.Entity
{
    internal class EntityGetAttributesCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            storageFrame.Stack.Push(parameters[0]);
            Debug.WriteLine($"[↑ {storageFrame.Stack.Count}] Return value from {method.FullName}");
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            method.FullName == "Microsoft.Xrm.Sdk.AttributeCollection Microsoft.Xrm.Sdk.Entity::get_Attributes()";
    }
}
