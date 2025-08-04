using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class EntityGetAttributesCallHook : ICallHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, Stack<object> stack)
        {
            stack.Push(parameters[0]);
            Debug.WriteLine($"[↑ {stack.Count}] Return value from {method.FullName}");
            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "Microsoft.Xrm.Sdk.AttributeCollection Microsoft.Xrm.Sdk.Entity::get_Attributes()";
    }
}
