using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class EntityGetAttributesCallHook : ICallHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            vmData.Stack.Push(parameters[0]);
            Debug.WriteLine($"[↑ {vmData.Stack.Count}] Return value from {method.FullName}");
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "Microsoft.Xrm.Sdk.AttributeCollection Microsoft.Xrm.Sdk.Entity::get_Attributes()";
    }
}
