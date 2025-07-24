using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph.Hooks.Creations
{
    internal class ProxyCreationHook : ICreationHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var entity = new EntityObj();
            var vm = new PluginGraphVM(vmData.ModuleList);
            var (apicalls, returnValue) = vm.Execute(methodDef!, [entity]);

            vmData.Stack.Push(entity);
            vmData.ApiCalls.AddRange(apicalls);
            if (returnValue != null)
            {
                vmData.Stack.Push(returnValue);
                Debug.WriteLine($"[↑ {vmData.Stack.Count}] Return value from {method.FullName}");
            }
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            methodDef != null && methodDef.DeclaringType.BaseType?.FullName == "Microsoft.Xrm.Sdk.Entity";
    }
}
