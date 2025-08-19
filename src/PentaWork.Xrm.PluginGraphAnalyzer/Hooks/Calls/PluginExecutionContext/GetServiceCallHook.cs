using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.PluginExecutionContext
{
    internal class GetServiceCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            storageFrame.Stack.Push(new ExecutionContextObj());
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            method.FullName == "System.Object System.IServiceProvider::GetService(System.Type)"
            && parameters.Count > 1
            && (parameters[1] as GenericObj)?.Parameters.SingleOrDefault()?.ToString().Contains("ldtoken Microsoft.Xrm.Sdk.IPluginExecutionContext") == true;
    }
}
